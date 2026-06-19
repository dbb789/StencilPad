using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class TextTool : ITool
{
    public class Factory(Sheet Sheet,
                         OverlayContainer OverlayContainer,
                         IUnitSnapOverlay UnitSnapOverlay,
                         IOperationService OperationService,
                         Factory<TextToolOverlay> OverlayFactory) : IToolFactory
    {
        public string IconResource => "TextTool";
        public string Tooltip => "Text";

        public ITool Create(IToolButton button)
        {
            return new TextTool(Sheet, OverlayContainer, UnitSnapOverlay, OperationService, OverlayFactory);
        }
    }

    private readonly Sheet _sheet;
    private readonly OverlayContainer _overlayContainer;
    private readonly IUnitSnapOverlay _unitSnapOverlay;
    private readonly IOperationService _operationService;
    private readonly Factory<TextToolOverlay> _overlayFactory;
    private TextToolOverlay? _overlay;

    private TextTool(Sheet sheet,
                     OverlayContainer overlayContainer,
                     IUnitSnapOverlay unitSnapOverlay,
                     IOperationService operationService,
                     Factory<TextToolOverlay> overlayFactory)
    {
        _sheet = sheet;
        _overlayContainer = overlayContainer;
        _unitSnapOverlay = unitSnapOverlay;
        _operationService = operationService;
        _overlayFactory = overlayFactory;
    }

    public void Dispose()
    { }

    public void ToolBegin()
    {
        _overlay = _overlayFactory.Create();
        _overlayContainer.ActiveOverlay = _overlay;
        _unitSnapOverlay.Begin();

        _overlay.OnTextPlaced += TextPlaced;
        _overlay.OnTextUpdated += TextUpdated;
    }

    public void ToolEnd()
    {
        _overlayContainer.ActiveOverlay = null;
        _unitSnapOverlay.End();

        if (_overlay is not null)
        {
            _overlay.Commit();
            _overlay.OnTextPlaced -= TextPlaced;
            _overlay.OnTextUpdated -= TextUpdated;
            _overlay.Dispose();
            _overlay = null;
        }
    }

    private void TextPlaced(UnitBounds bounds, string text)
    {
        var element = new TextElement(bounds, text);

        _operationService.Push(new AddSheetElementOperation(_sheet.Id, element));
    }

    private void TextUpdated(TextElement element, string text)
    {
        element.Text = text;
    }
}
