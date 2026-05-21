using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class MarkerPathTool : ITool
{
    public class Factory(IOperationService OperationService) : IToolFactory
    {
        public string IconResource => "MarkerPathTool";
        public string Tooltip => "Marker Path";
        
        public ITool Create(IToolButton _, Sheet sheet, IToolContext context)
        {
            return new MarkerPathTool(sheet, context, OperationService);
        }
    }

    private readonly Sheet _sheet;
    private readonly IToolContext _context;
    private readonly IOperationService _operationService;
    private ShapeToolOverlay? _overlay;

    private MarkerPathTool(Sheet sheet,
                           IToolContext context,
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
        _context.UnitSnapOverlay.Begin();

        _overlay.OnPolygonCompleted += PolygonCompleted;
    }

    public void ToolEnd()
    {
        _context.ToolOverlay.ActiveOverlay = null;
        _context.UnitSnapOverlay.End();

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
                                         new MarkerPath(polygon)));
    }
}
