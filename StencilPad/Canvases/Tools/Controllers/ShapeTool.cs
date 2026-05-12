using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class ShapeTool : ITool
{
    public class Factory(IOperationService OperationService) : IToolFactory
    {
        public string IconResource => "StraightLineTool";
        public string Tooltip => "Shape";
        
        public ITool Create(IToolButton _, Sheet sheet, IToolContext context)
        {
            return new ShapeTool(sheet, context, OperationService);
        }
    }

    private readonly Sheet _sheet;
    private readonly IToolContext _context;
    private readonly IOperationService _operationService;
    private ShapeToolOverlay? _overlay;

    private ShapeTool(Sheet sheet, IToolContext context,
                      IOperationService operationService)
    {
        _sheet = sheet;
        _context = context;
        _operationService = operationService;
    }

    public void Dispose()
    { }
    
    public void ToolBegin()
    {
        _overlay = new ShapeToolOverlay(_context.Viewport, _context.UnitSnap);
        _context.ToolOverlay.ActiveOverlay = _overlay;

        _overlay.OnPolygonCompleted += PolygonCompleted;
    }

    public void ToolEnd()
    {
        _context.ToolOverlay.ActiveOverlay = null;

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
