using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Rendering;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class TextTool : ITool
{
    public class Factory(Sheet Sheet,
                         ToolOverlay ToolOverlay,
                         IUnitSnapOverlay UnitSnapOverlay,
                         IOperationService OperationService,
                         Factory<TextToolOverlay> OverlayFactory) : IToolFactory
    {
        public string IconResource => "TextTool";
        public string Tooltip => "Text";

        public ITool Create(IToolButton button)
        {
            return new TextTool(Sheet, ToolOverlay, UnitSnapOverlay, OperationService, OverlayFactory);
        }
    }

    private readonly Sheet _sheet;
    private readonly ToolOverlay _toolOverlay;
    private readonly IUnitSnapOverlay _unitSnapOverlay;
    private readonly IOperationService _operationService;
    private readonly Factory<TextToolOverlay> _overlayFactory;
    private TextToolOverlay? _overlay;

    private TextTool(Sheet sheet,
                     ToolOverlay toolOverlay,
                     IUnitSnapOverlay unitSnapOverlay,
                     IOperationService operationService,
                     Factory<TextToolOverlay> overlayFactory)
    {
        _sheet = sheet;
        _toolOverlay = toolOverlay;
        _unitSnapOverlay = unitSnapOverlay;
        _operationService = operationService;
        _overlayFactory = overlayFactory;
    }

    public void Dispose()
    { }

    public void ToolBegin()
    {
        _overlay = _overlayFactory.Create();
        _toolOverlay.ActiveOverlay = _overlay;
        _unitSnapOverlay.Begin();

        _overlay.OnTextPlaced += TextPlaced;
    }

    public void ToolEnd()
    {
        _toolOverlay.ActiveOverlay = null;
        _unitSnapOverlay.End();

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
        element.Max = position + TextElementRenderer.Measure(element);

        _operationService.Push(
            new AddSheetElementOperation(_sheet.Id, element));
    }
}
