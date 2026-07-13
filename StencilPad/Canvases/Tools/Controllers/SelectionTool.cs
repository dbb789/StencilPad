using Microsoft.Extensions.Logging;
using StencilPad.Canvases.Tools.Actions;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Resolvers;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class SelectionTool : ITool
{
    public class Factory(ILogger<SelectionTool> Logger,
                         Sheet Sheet,
                         OverlayContainer OverlayContainer,
                         ISettings Settings,
                         IRubberBand RubberBand,
                         SheetResolver SheetResolver,
                         IHintService HintService,
                         IModelPropertiesService ModelPropertiesService,
                         IOperationService OperationService,
                         Factory<SelectionToolOverlay> OverlayFactory) : IToolFactory
    {
        public string IconResource => "SelectionTool";
        public string Tooltip => "Select";

        public ITool Create(IToolButton button)
        {
            return new SelectionTool(Logger,
                                     Sheet,
                                     OverlayContainer,
                                     Settings,
                                     RubberBand,
                                     SheetResolver,
                                     HintService,
                                     ModelPropertiesService,
                                     OperationService,
                                     OverlayFactory);
        }
    }

    private readonly ILogger<SelectionTool> _logger;
    private readonly Sheet _sheet;
    private readonly OverlayContainer _overlayContainer;
    private readonly ISettings _settings;
    private readonly IRubberBand _rubberBand;
    private readonly SheetResolver _sheetResolver;
    private readonly IHintService _hintService;
    private readonly IModelPropertiesService _modelPropertiesService;
    private readonly IOperationService _operationService;
    private readonly Factory<SelectionToolOverlay> _overlayFactory;

    private SelectionToolOverlay? _overlay;
    private Dictionary<ISheetElement, UnitBounds> _resizeInitialBounds = new();
    private decimal _rotateAccumulatedAngle;
    private decimal _rotateLastSnappedAngle;

    private IDisposable? _editContext;
    
    private SelectionTool(ILogger<SelectionTool> logger,
                          Sheet sheet,
                          OverlayContainer overlayContainer,
                          ISettings settings,
                          IRubberBand rubberBand,
                          SheetResolver sheetResolver,
                          IHintService hintService,
                          IModelPropertiesService modelPropertiesService,
                          IOperationService operationService,
                          Factory<SelectionToolOverlay> overlayFactory)
    {
        _logger = logger;
        _sheet = sheet;
        _overlayContainer = overlayContainer;
        _settings = settings;
        _rubberBand = rubberBand;
        _sheetResolver = sheetResolver;
        _hintService = hintService;
        _modelPropertiesService = modelPropertiesService;
        _operationService = operationService;
        _overlayFactory = overlayFactory;
    }

    public void Dispose()
    { }

    public void ToolBegin()
    {
        _overlay = _overlayFactory.Create();
        _overlayContainer.ActiveOverlay = _overlay;

        _rubberBand.IsActive = true;
        _rubberBand.PointSelected += PointSelected;
        _rubberBand.BoundsSelected += BoundsSelected;

        _overlay.ActionInvoked += ActionInvoked;
        
        _overlay.SelectionDragStarted += SelectionDragStarted;
        _overlay.SelectionDragged += SelectionDragged;
        _overlay.SelectionDragEnded += SelectionDragEnded;
        
        _overlay.SelectionResizeStarted += SelectionResizeStarted;
        _overlay.SelectionResized += SelectionResized;
        _overlay.SelectionResizeEnded += SelectionResizeEnded;
        
        _overlay.SelectionRotateStarted += SelectionRotateStarted;
        _overlay.SelectionRotated += SelectionRotated;
        _overlay.SelectionRotateEnded += SelectionRotateEnded;
    }

    public void ToolEnd()
    {
        _operationService.FlushEditContext();
        _hintService.ClearHint();

        _rubberBand.IsActive = false;

        _overlayContainer.ActiveOverlay = null;

        if (_overlay is not null)
        {
            _overlay.ActionInvoked -= ActionInvoked;
            
            _overlay.SelectionDragStarted -= SelectionDragStarted;
            _overlay.SelectionDragged -= SelectionDragged;
            _overlay.SelectionDragEnded -= SelectionDragEnded;

            _overlay.SelectionResizeStarted -= SelectionResizeStarted;
            _overlay.SelectionResized -= SelectionResized;
            _overlay.SelectionResizeEnded -= SelectionResizeEnded;
            
            _overlay.SelectionRotateStarted -= SelectionRotateStarted;
            _overlay.SelectionRotated -= SelectionRotated;
            _overlay.SelectionRotateEnded -= SelectionRotateEnded;
            
            _overlay.Dispose();
            _overlay = null;
        }

        _rubberBand.PointSelected -= PointSelected;
        _rubberBand.BoundsSelected -= BoundsSelected;
    }

    private void PointSelected(Unit2D point)
    {
        // This needs to both cycle through everything under the mouse and also
        // modify the selection based on modifier keys so the logic is a bit
        // convoluted.
        
        // Firstly, let's find everything under the mouse and put it in a list,
        // topmost first.
        var hitList = new List<ISheetElement>(8);

        foreach (var resolver in _sheetResolver.Elements.Reverse())
        {
            if (resolver.OutlineContainsPoint(point))
            {
                hitList.Add(resolver.Element);
            }
        }
        
        ISheetElement? lastSelection = null;

        foreach (var hit in hitList)
        {
            if (_sheet.Selection.Contains(hit))
            {
                // Next we want to find the first available selected item that
                // was under the mouse.
                if (lastSelection is null)
                {
                    lastSelection = hit;
                }

                // And if we're modifying the selection, we want to remove
                // everything else so that we can re-add the next item as
                // necessary.
                if (ModifierUtil.IsModifyingSelection())
                {
                    _sheet.Selection.Remove(hit);
                }
            }
        }

        // But if we're not modifying the selection, just clear out the lot.
        if (!ModifierUtil.IsModifyingSelection())
        {
            _sheet.Selection.Clear();
        }

        // Next find the next item in the hit list after the last selected item
        // and select it.
        var currentIndex = (lastSelection != null) ? hitList.IndexOf(lastSelection) : -1;

        ++currentIndex;

        if (currentIndex >= 0 && currentIndex < hitList.Count)
        {
            _sheet.Selection.Add(hitList[currentIndex]);
        }
    }

    private void BoundsSelected(UnitBounds bounds)
    {
        if (!ModifierUtil.IsModifyingSelection())
        {
            _sheet.Selection.Clear();
        }

        foreach (var resolver in _sheetResolver.Elements)
        {
            var selectionBounds = resolver.GetOutlineBounds();
            
            if (bounds.Contains(selectionBounds))
            {
                _sheet.Selection.Add(resolver.Element);
            }
        }
    }

    ////////////////////////////////////////
    
    private void SelectionDragStarted()
    {
        StartEditContext();
    }
    
    private void SelectionDragged(Unit2D totalDelta, Unit2D delta)
    {
        foreach (var selected in _sheet.Selection)
        {
            selected.Transform = selected.Transform with
                { Position = selected.Transform.Position + delta };
        }

        _hintService.SetHint($"Move: {UnitUtil.FormatSuffixScaled(totalDelta.X, _settings.UnitSettings)}, {UnitUtil.FormatSuffixScaled(totalDelta.Y, _settings.UnitSettings)}");
    }
    
    private void SelectionDragEnded()
    {
        FlushEditContext();
        _hintService.ClearHint();
    }

    ////////////////////////////////////////
    
    private void SelectionResizeStarted()
    {
        StartEditContext();

        _resizeInitialBounds.Clear();

        foreach (var selected in _sheet.Selection)
        {
            _resizeInitialBounds[selected] = selected.GetBounds();
        }
    }

    private void SelectionResized(ISheetElement draggedElement, Unit2D seDelta)
    {
        // Clamp resize at minimum for all elements for sanity.
        foreach (var selected in _sheet.Selection)
        {
            if (!_resizeInitialBounds.TryGetValue(selected, out var initialBounds))
            {
                continue;
            }

            seDelta = new Unit2D(Unit.Max(Unit.FromMillimeters(0.1) - initialBounds.Size.X, seDelta.X),
                                 Unit.Min(initialBounds.Size.Y - Unit.FromMillimeters(0.1), seDelta.Y));
        }

        foreach (var selected in _sheet.Selection)
        {
            if (!_resizeInitialBounds.TryGetValue(selected, out var initialBounds))
            {
                continue;
            }

            var newBounds = UnitBounds.FromMinMax(initialBounds.Min + new Unit2D(Unit.Zero, seDelta.Y),
                                                  initialBounds.Max + new Unit2D(seDelta.X, Unit.Zero));


            selected.SetTransformedBounds(newBounds, selected.Transform);
        }

        var size = draggedElement.GetBounds().Size;
        
        _hintService.SetHint($"Resize: {UnitUtil.FormatSuffixScaled(size.X, _settings.UnitSettings)} x {UnitUtil.FormatSuffixScaled(size.Y, _settings.UnitSettings)}");
    }
    
    private void SelectionResizeEnded()
    {
        FlushEditContext();
        _hintService.ClearHint();
    }

    ////////////////////////////////////////
    
    private void SelectionRotateStarted()
    {
        StartEditContext();

        _rotateAccumulatedAngle = 0m;
        _rotateLastSnappedAngle = 0m;

        foreach (var selected in _sheet.Selection)
        {
            selected.NormalizePosition();
        }
    }

    private void SelectionRotated(double totalRadians, double deltaRadians)
    {
        _rotateAccumulatedAngle += (decimal)(deltaRadians * (180.0 / Math.PI));

        decimal effectiveDelta;

        effectiveDelta = _rotateAccumulatedAngle - _rotateLastSnappedAngle;
        _rotateLastSnappedAngle = _rotateAccumulatedAngle;

        if (effectiveDelta == 0m)
        {
            return;
        }

        foreach (var selected in _sheet.Selection)
        {
            selected.Transform = selected.Transform with
                { Angle = selected.Transform.Angle + effectiveDelta };
        }

        _hintService.SetHint($"Rotate: {totalRadians * MathUtil.Rad2Deg:0.##}°");
    }

    private void SelectionRotateEnded()
    {
        FlushEditContext();
        _hintService.ClearHint();
    }

    ////////////////////////////////////////

    private void StartEditContext()
    {
        if (_editContext is not null)
        {
            _logger.LogError("Starting new edit context without flushing previous one.");

            // _sheet.Selection has possibly changed since the last edit context
            // was created, so we should probably just flush it before starting
            // a new one to avoid losing changes.
            FlushEditContext();
        }
        
        _editContext = _operationService.CreateEditContext(_sheet, _sheet.Selection);
    }

    private void FlushEditContext()
    {
        if (_editContext is not null)
        {
            _editContext.Dispose();
            _editContext = null;
        }
    }
    
    private void ActionInvoked(ISheetElementAction action)
    {
        action.Invoke(_sheet, _sheet.Selection);
    }
}
