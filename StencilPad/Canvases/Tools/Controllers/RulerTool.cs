using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class RulerTool : ITool
{
    public class Factory(IOperationService OperationService) : IToolFactory
    {
        public string IconResource => "RulerTool";
        public string Tooltip => "Ruler";

        public ITool Create(IToolButton button,
                            Sheet sheet,
                            IToolContext context)
        {
            return new RulerTool(sheet, context, OperationService);
        }
    }

    private readonly Sheet _sheet;
    private readonly IToolContext _context;
    private readonly IOperationService _operationService;
    private RulerToolOverlay? _overlay;

    private RulerTool(Sheet sheet,
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
        _overlay = new RulerToolOverlay(_context.Viewport, _context.UnitSnap);
        _context.ToolOverlay.ActiveOverlay = _overlay;

        _overlay.OnRulerPlaced += RulerPlaced;
    }

    public void ToolEnd()
    {
        _context.ToolOverlay.ActiveOverlay = null;

        if (_overlay is not null)
        {
            _overlay.OnRulerPlaced -= RulerPlaced;
            _overlay.Dispose();
            _overlay = null;
        }
    }

    private void RulerPlaced(Unit2D start, Unit2D end)
    {
        _operationService.Push(
            new AddSheetElementOperation(_sheet.Id, new Ruler(start, end)));
    }
}
