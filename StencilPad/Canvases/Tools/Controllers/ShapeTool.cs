using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class ShapeTool : ITool
{
    public class Factory(Sheet Sheet,
                         ToolOverlay ToolOverlay,
                         IViewport Viewport,
                         IUnitSnap UnitSnap,
                         IUnitSnapOverlay UnitSnapOverlay,
                         IOperationService OperationService) : IToolFactory
    {
        public string IconResource => "StraightLineTool";
        public string Tooltip => "Shape";

        public ITool Create(IToolButton button)
        {
            return new ShapeTool(Sheet,
                                 ToolOverlay,
                                 Viewport,
                                 UnitSnap,
                                 UnitSnapOverlay,
                                 OperationService);
        }
    }

    private readonly Sheet _sheet;
    private readonly ToolOverlay _toolOverlay;
    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;
    private readonly IUnitSnapOverlay _unitSnapOverlay;
    private readonly IOperationService _operationService;
    private ShapeToolOverlay? _overlay;

    private ShapeTool(Sheet sheet,
                      ToolOverlay toolOverlay,
                      IViewport viewport,
                      IUnitSnap unitSnap,
                      IUnitSnapOverlay unitSnapOverlay,
                      IOperationService operationService)
    {
        _sheet = sheet;
        _toolOverlay = toolOverlay;
        _viewport = viewport;
        _unitSnap = unitSnap;
        _unitSnapOverlay = unitSnapOverlay;
        _operationService = operationService;
    }

    public void Dispose()
    { }

    public void ToolBegin()
    {
        _overlay = new ShapeToolOverlay(_viewport, _unitSnap);
        _toolOverlay.ActiveOverlay = _overlay;
        _unitSnapOverlay.Begin();

        _overlay.OnPolygonCompleted += PolygonCompleted;
    }

    public void ToolEnd()
    {
        _toolOverlay.ActiveOverlay = null;
        _unitSnapOverlay.End();

        if (_overlay is not null)
        {
            _overlay.OnPolygonCompleted -= PolygonCompleted;
            _overlay.Dispose();
            _overlay = null;
        }
    }

    private void PolygonCompleted(Polygon polygon)
    {
        _operationService.Push(
            new AddSheetElementOperation(_sheet.Id,
                                         new Shape(polygon)));
    }
}
