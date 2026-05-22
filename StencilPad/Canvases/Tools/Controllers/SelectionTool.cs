using StencilPad.Canvases.Tools.Actions;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;
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
                         Factory<SelectionToolOverlay> OverlayFactory) : IToolFactory
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
    private readonly Factory<SelectionToolOverlay> _overlayFactory;

    private SelectionToolOverlay? _overlay;
    private Dictionary<ISheetElement, UnitBounds> _resizeInitialBounds = new();
    private decimal _rotateAccumulatedAngle;
    private decimal _rotateLastSnappedAngle;

    private const decimal AngleSnapDegrees = 15m;

    private SelectionTool(Sheet sheet,
                          ToolOverlay toolOverlay,
                          IRubberBand rubberBand,
                          IModelPropertiesService modelPropertiesService,
                          Factory<SelectionToolOverlay> overlayFactory)
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

        _rubberBand.IsActive = true;
        _rubberBand.PointSelected += PointSelected;
        _rubberBand.BoundsSelected += BoundsSelected;

        _overlay.ActionInvoked += ActionInvoked;
        _overlay.SelectionDragged += SelectionDragged;
        _overlay.SelectionResizeStarted += SelectionResizeStarted;
        _overlay.SelectionResized += SelectionResized;
        _overlay.SelectionRotateStarted += SelectionRotateStarted;
        _overlay.SelectionRotated += SelectionRotated;
    }

    public void ToolEnd()
    {
        _rubberBand.IsActive = false;

        _toolOverlay.ActiveOverlay = null;

        if (_overlay is not null)
        {
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
        _rotateAccumulatedAngle = 0m;
        _rotateLastSnappedAngle = 0m;

        foreach (var selected in _sheet.Selection)
        {
            selected.NormalizePosition();
        }
    }

    private void SelectionRotated(double deltaRadians)
    {
        _rotateAccumulatedAngle += (decimal)(deltaRadians * (180.0 / Math.PI));

        decimal effectiveDelta;

        if (ModifierUtil.IsAngleSnap())
        {
            var snapped = Math.Round(_rotateAccumulatedAngle / AngleSnapDegrees) * AngleSnapDegrees;
            effectiveDelta = snapped - _rotateLastSnappedAngle;
            _rotateLastSnappedAngle = snapped;
        }
        else
        {
            effectiveDelta = _rotateAccumulatedAngle - _rotateLastSnappedAngle;
            _rotateLastSnappedAngle = _rotateAccumulatedAngle;
        }

        if (effectiveDelta == 0m)
        {
            return;
        }

        foreach (var selected in _sheet.Selection)
        {
            selected.Transform = selected.Transform with { Angle = selected.Transform.Angle + effectiveDelta };
        }
    }

    private void ActionInvoked(ISheetElementAction action)
    {
        action.Invoke(_sheet, _sheet.Selection);
    }
}
