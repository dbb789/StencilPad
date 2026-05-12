using StencilPad.Canvases.Rendering;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class TextTool : ITool
{
    public class Factory(IOperationService OperationService) : IToolFactory
    {
        public string IconResource => "TextTool";
        public string Tooltip => "Text";

        public ITool Create(IToolButton _, Sheet sheet, IToolContext context)
        {
            return new TextTool(sheet, context, OperationService);
        }
    }

    private readonly Sheet _sheet;
    private readonly IToolContext _context;
    private readonly IOperationService _operationService;
    private TextToolOverlay? _overlay;

    private TextTool(Sheet sheet, IToolContext context, IOperationService operationService)
    {
        _sheet = sheet;
        _context = context;
        _operationService = operationService;
    }

    public void Dispose()
    { }

    public void ToolBegin()
    {
        _overlay = new TextToolOverlay(_context.Viewport, _context.UnitSnap);
        _context.ToolOverlay.ActiveOverlay = _overlay;
        _overlay.OnTextPlaced += TextPlaced;
    }

    public void ToolEnd()
    {
        _context.ToolOverlay.ActiveOverlay = null;

        if (_overlay is not null)
        {
            _overlay.Commit();
            _overlay.OnTextPlaced -= TextPlaced;
            _overlay.Dispose();
            _overlay = null;
        }
    }

    private void TextPlaced(Unit2D position, string text)
    {
        var element = new TextElement(position, text);
        element.End = position + TextElementRenderer.Measure(element);

        _operationService.Push(
            new AddSheetElementOperation(_sheet.Id, element));
    }
}
