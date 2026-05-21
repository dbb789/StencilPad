using StencilPad.Canvases.Tools.Actions;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class SelectionTool : ITool
{
    public class Factory : IToolFactory
    {
        public string IconResource => "SelectionTool";
        public string Tooltip => "Select";

        private readonly IModelPropertiesService _modelPropertiesService;
        private readonly SheetElementActionSet _sheetElementActionSet;
        
        public Factory(IModelPropertiesService modelPropertiesService,
                       SheetElementActionSet sheetElementActionSet)
        {
            _modelPropertiesService = modelPropertiesService;
            _sheetElementActionSet = sheetElementActionSet;
        }

        public ITool Create(IToolButton _, Sheet sheet, IToolContext context)
        {
            return new SelectionTool(sheet,
                                     context,
                                     _modelPropertiesService,
                                     _sheetElementActionSet);
        }
    }

    private readonly Sheet _sheet;
    private readonly IToolContext _context;
    private readonly IModelPropertiesService _modelPropertiesService;
    private readonly SheetElementActionSet _sheetElementActionSet;
    
    private SelectionToolOverlay? _overlay;

    private SelectionTool(Sheet sheet,
                          IToolContext context,
                          IModelPropertiesService modelPropertiesService,
                          SheetElementActionSet sheetElementActionSet)
    {
        _sheet = sheet;
        _context = context;
        _modelPropertiesService = modelPropertiesService;
        _sheetElementActionSet = sheetElementActionSet;
    }

    public void Dispose()
    { }

    public void ToolBegin()
    {
        _overlay = new SelectionToolOverlay(_context,
                                            _sheet,
                                            _sheetElementActionSet.Actions);
        _context.ToolOverlay.ActiveOverlay = _overlay;

        _context.RubberBand.PointSelected += PointSelected;
        _context.RubberBand.BoundsSelected += BoundsSelected;
        _context.SelectAllRequested += SelectAll;
        _context.ClearSelectionRequested += ClearSelection;

        _overlay.ActionInvoked += ActionInvoked;
        _overlay.SelectionDragged += SelectionDragged;
        _overlay.SelectionRotateStarted += SelectionRotateStarted;
        _overlay.SelectionRotated += SelectionRotated;
    }

    public void ToolEnd()
    {
        _context.ToolOverlay.ActiveOverlay = null;

        if (_overlay is not null)
        {
            _context.RubberBand.PointSelected -= PointSelected;
            _context.RubberBand.BoundsSelected -= BoundsSelected;
            _context.SelectAllRequested -= SelectAll;
            _context.ClearSelectionRequested -= ClearSelection;

            _overlay.ActionInvoked -= ActionInvoked;
            _overlay.SelectionDragged -= SelectionDragged;
            _overlay.SelectionRotateStarted -= SelectionRotateStarted;
            _overlay.SelectionRotated -= SelectionRotated;
            _overlay.Dispose();
            _overlay = null;
        }
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
        action.Invoke(_context, _sheet, _sheet.Selection);
    }
}
