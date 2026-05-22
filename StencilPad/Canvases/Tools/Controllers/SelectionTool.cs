using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Actions;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class SelectionTool : ITool
{
    public class Factory(Sheet Sheet,
                         ToolOverlay ToolOverlay,
                         IRubberBand RubberBand,
                         IModelPropertiesService ModelPropertiesService,
                         SelectionToolOverlay.Factory OverlayFactory) : IToolFactory
    {
        public string IconResource => "SelectionTool";
        public string Tooltip => "Select";

        public ITool Create(IToolButton button)
        {
            return new SelectionTool(Sheet,
                                     ToolOverlay,
                                     RubberBand,
                                     ModelPropertiesService,
                                     OverlayFactory);
        }
    }

    private readonly Sheet _sheet;
    private readonly ToolOverlay _toolOverlay;
    private readonly IRubberBand _rubberBand;
    private readonly IModelPropertiesService _modelPropertiesService;
    private readonly SelectionToolOverlay.Factory _overlayFactory;

    private SelectionToolOverlay? _overlay;
    private Dictionary<ISheetElement, UnitBounds> _resizeInitialBounds = new();

    private SelectionTool(Sheet sheet,
                          ToolOverlay toolOverlay,
                          IRubberBand rubberBand,
                          IModelPropertiesService modelPropertiesService,
                          SelectionToolOverlay.Factory overlayFactory)
    {
        _sheet = sheet;
        _toolOverlay = toolOverlay;
        _rubberBand = rubberBand;
        _modelPropertiesService = modelPropertiesService;
        _overlayFactory = overlayFactory;
    }

    public void Dispose()
    { }

    public void ToolBegin()
    {
        _overlay = _overlayFactory.Create();
        _toolOverlay.ActiveOverlay = _overlay;

        _rubberBand.PointSelected += PointSelected;
        _rubberBand.BoundsSelected += BoundsSelected;
        // TODO: SelectAllRequested / ClearSelectionRequested need refactoring
        // _context.SelectAllRequested += SelectAll;
        // _context.ClearSelectionRequested += ClearSelection;

        _overlay.ActionInvoked += ActionInvoked;
        _overlay.SelectionDragged += SelectionDragged;
        _overlay.SelectionResizeStarted += SelectionResizeStarted;
        _overlay.SelectionResized += SelectionResized;
        _overlay.SelectionRotateStarted += SelectionRotateStarted;
        _overlay.SelectionRotated += SelectionRotated;
    }

    public void ToolEnd()
    {
        _toolOverlay.ActiveOverlay = null;

        if (_overlay is not null)
        {
            // _context.SelectAllRequested -= SelectAll;
            // _context.ClearSelectionRequested -= ClearSelection;

            _overlay.ActionInvoked -= ActionInvoked;
            _overlay.SelectionDragged -= SelectionDragged;
            _overlay.SelectionResizeStarted -= SelectionResizeStarted;
            _overlay.SelectionResized -= SelectionResized;
            _overlay.SelectionRotateStarted -= SelectionRotateStarted;
            _overlay.SelectionRotated -= SelectionRotated;
            _overlay.Dispose();
            _overlay = null;
        }

        _rubberBand.PointSelected -= PointSelected;
        _rubberBand.BoundsSelected -= BoundsSelected;
    }

    private void PointSelected(Unit2D point)
    {
        ISheetElement? lastSelection = null;

        if (_sheet.Selection.Count == 1)
        {
            lastSelection = _sheet.Selection.FirstOrDefault();
        }

        if (!ModifierUtil.IsModifyingSelection())
        {
            _sheet.Selection.Clear();
        }

        var hitList = new List<ISheetElement>(8);

        foreach (var element in _sheet.Elements)
        {
            if (element.GetTransformedBounds().Contains(point))
            {
                hitList.Add(element);
            }
        }

        if (hitList.Count == 0)
        {
            return;
        }

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

        foreach (var element in _sheet.Elements)
        {
            if (element.GetTransformedBounds().Intersects(bounds))
            {
                _sheet.Selection.Add(element);
            }
        }
    }

    private void SelectionDragged(Unit2D delta)
    {
        foreach (var selected in _sheet.Selection)
        {
            selected.Translate(delta);
        }
    }

    private void SelectionResizeStarted()
    {
        _resizeInitialBounds.Clear();

        foreach (var selected in _sheet.Selection)
        {
            _resizeInitialBounds[selected] = selected.GetTransformedBounds();
        }
    }

    private void SelectionResized(Unit2D seDelta)
    {
        foreach (var selected in _sheet.Selection)
        {
            if (!_resizeInitialBounds.TryGetValue(selected, out var initialBounds))
            {
                continue;
            }

            var newBounds = UnitBounds.FromMinMax(initialBounds.Min, initialBounds.SE + seDelta);
            selected.SetBounds(newBounds, selected.Transform);
        }
    }

    private void SelectionRotateStarted()
    {
        foreach (var selected in _sheet.Selection)
        {
            selected.NormalizePosition();
        }
    }

    private void SelectionRotated(double deltaRadians)
    {
        var deltaDegrees = (decimal)(deltaRadians * (180.0 / Math.PI));
        foreach (var selected in _sheet.Selection)
        {
            selected.Transform = selected.Transform with { Angle = selected.Transform.Angle + deltaDegrees };
        }
    }

    private void SelectAll()
    {
        _sheet.Selection.Clear();

        foreach (var element in _sheet.Elements)
        {
            _sheet.Selection.Add(element);
        }
    }

    private void ClearSelection()
    {
        _sheet.Selection.Clear();
    }

    private void ActionInvoked(ISheetElementAction action)
    {
        action.Invoke(_sheet, _sheet.Selection);
    }
}
