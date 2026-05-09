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

        public Factory(IModelPropertiesService modelPropertiesService)
        {
            _modelPropertiesService = modelPropertiesService;
        }

        public ITool Create(IToolButton _, Sheet sheet, IToolContext context)
        {
            return new SelectionTool(sheet, context, _modelPropertiesService);
        }
    }

    private readonly Sheet _sheet;
    private readonly IToolContext _context;
    private readonly IModelPropertiesService _modelPropertiesService;
    private SelectionToolOverlay? _overlay;

    private SelectionTool(Sheet sheet,
                          IToolContext context,
                          IModelPropertiesService modelPropertiesService)
    {
        _sheet = sheet;
        _context = context;
        _modelPropertiesService = modelPropertiesService;
    }

    public void Dispose()
    { }

    public void ToolBegin()
    {
        _overlay = new SelectionToolOverlay(_context.SheetRenderer,
                                            _sheet,
                                            _context.Viewport,
                                            _context.UnitSnap);
        _context.ToolOverlay.ActiveOverlay = _overlay;

        _context.RubberBand.PointSelected += PointSelected;
        _context.RubberBand.BoundsSelected += BoundsSelected;
        _context.SelectAllRequested += SelectAll;
        _context.ClearSelectionRequested += ClearSelection;
        _overlay.PointSelected += PointSelected;
        _overlay.SelectionDragged += SelectionDragged;
        _overlay.ShowProperties += ShowProperties;
        _overlay.MirrorX += MirrorX;
        _overlay.MirrorY += MirrorY;
        _overlay.JustifyTop += JustifyTop;
        _overlay.JustifyMiddle += JustifyMiddle;
        _overlay.JustifyBottom += JustifyBottom;
        _overlay.JustifyLeft += JustifyLeft;
        _overlay.JusifyCentre += JustifyCentre;
        _overlay.JustifyRight += JustifyRight;
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
            _overlay.PointSelected -= PointSelected;
            _overlay.SelectionDragged -= SelectionDragged;
            _overlay.ShowProperties -= ShowProperties;
            _overlay.MirrorX -= MirrorX;
            _overlay.MirrorY -= MirrorY;
            _overlay.JustifyTop -= JustifyTop;
            _overlay.JustifyMiddle -= JustifyMiddle;
            _overlay.JustifyBottom -= JustifyBottom;
            _overlay.JustifyLeft -= JustifyLeft;
            _overlay.JusifyCentre -= JustifyCentre;
            _overlay.JustifyRight -= JustifyRight;
            _overlay.Dispose();
            _overlay = null;
        }
    }

    private void PointSelected(Unit2D point)
    {
        ISheetElement? lastSelection = null;

        if (_sheet.Selection.Count == 1)
        {
            lastSelection = _sheet.Selection[0];
        }

        _sheet.Selection.Clear();
        
        var hitList = new List<ISheetElement>(8);
        
        for (int i = _context.SheetRenderer.Count - 1; i >= 0; --i)
        {
            var element = _context.SheetRenderer[i];
            
            if (element.HitTest(point))
            {
                hitList.Add(element.Element);
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
        _sheet.Selection.Clear();

        for (int i = _context.SheetRenderer.Count - 1; i >= 0; --i)
        {
            var element = _context.SheetRenderer[i];

            if (element.BoundsTest(bounds))
            {
                _sheet.Selection.Add(element.Element);
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

    private void SelectAll()
    {
        _sheet.Selection.Clear();

        for (int i = _context.SheetRenderer.Count - 1; i >= 0; --i)
        {
            var element = _context.SheetRenderer[i];

            _sheet.Selection.Add(element.Element);
        }
    }

    private void ClearSelection()
    {
        _sheet.Selection.Clear();
    }

    private void ShowProperties()
    {
        var markerPaths = _sheet.Selection
            .OfType<MarkerPath>()
            .ToList();

        if (markerPaths.Count > 0)
        {
            _modelPropertiesService.ShowMarkerPathProperties(markerPaths);
        }
    }

    private void MirrorX()
    {
        var bounds = GetSelectionBounds();

        if (bounds is null)
        {
            return;
        }
        var centerY = bounds.Value.Center.Y;

        foreach (var element in _sheet.Selection.OfType<IPolygonSheetElement>())
        {
            element.EditablePolygon.MirrorX(centerY);
        }
    }

    private void MirrorY()
    {
        var bounds = GetSelectionBounds();

        if (bounds is null)
        {
            return;
        }
        
        var centerX = bounds.Value.Center.X;

        foreach (var element in _sheet.Selection.OfType<IPolygonSheetElement>())
        {
            element.EditablePolygon.MirrorY(centerX);
        }
    }

    private void JustifyTop()
    {
        Justify((selection, element) => new Unit2D(Unit.Zero, selection.Min.Y - element.Min.Y));
    }

    private void JustifyMiddle()
    {
        Justify((selection, element) => new Unit2D(Unit.Zero, selection.Center.Y - element.Center.Y));
    }

    private void JustifyBottom()
    {
        Justify((selection, element) => new Unit2D(Unit.Zero, selection.Max.Y - element.Max.Y));
    }

    private void JustifyLeft()
    {
        Justify((selection, element) => new Unit2D(selection.Min.X - element.Min.X, Unit.Zero));
    }

    private void JustifyCentre()
    {
        Justify((selection, element) => new Unit2D(selection.Center.X - element.Center.X, Unit.Zero));
    }

    private void JustifyRight()
    {
        Justify((selection, element) => new Unit2D(selection.Max.X - element.Max.X, Unit.Zero));
    }
    
    private void Justify(Func<UnitBounds, UnitBounds, Unit2D> getDelta)
    {
        var selectionBounds = GetSelectionBounds();
        
        if (selectionBounds is null)
        {
            return;
        }
        
        foreach (var element in _sheet.Selection)
        {
            if (_context.SheetRenderer.TryGetElementRenderer(element, out var renderer))
            {
                element.Translate(getDelta(selectionBounds.Value, renderer.SelectionBounds));
            }
        }
    }

    private UnitBounds? GetSelectionBounds()
    {
        UnitBounds? selectionBounds = null;

        foreach (var element in _sheet.Selection)
        {
            if (_context.SheetRenderer.TryGetElementRenderer(element, out var renderer))
            {
                selectionBounds = selectionBounds.HasValue
                    ? UnitBounds.Union(selectionBounds.Value, renderer.SelectionBounds)
                    : renderer.SelectionBounds;
            }
        }

        return selectionBounds;
    }
}
