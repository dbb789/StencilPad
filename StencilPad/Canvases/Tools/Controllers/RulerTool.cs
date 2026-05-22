using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class RulerTool : ITool
{
    public class Factory(Sheet Sheet,
                         ToolOverlay ToolOverlay,
                         IUnitSnapOverlay UnitSnapOverlay,
                         IOperationService OperationService,
                         RulerToolOverlay.Factory OverlayFactory) : IToolFactory
    {
        public string IconResource => "RulerTool";
        public string Tooltip => "Ruler";

        public ITool Create(IToolButton button)
        {
            return new RulerTool(Sheet, ToolOverlay, UnitSnapOverlay, OperationService, OverlayFactory);
        }
    }

    private readonly Sheet _sheet;
    private readonly ToolOverlay _toolOverlay;
    private readonly IUnitSnapOverlay _unitSnapOverlay;
    private readonly IOperationService _operationService;
    private readonly RulerToolOverlay.Factory _overlayFactory;
    private RulerToolOverlay? _overlay;

    private RulerTool(Sheet sheet,
                      ToolOverlay toolOverlay,
                      IUnitSnapOverlay unitSnapOverlay,
                      IOperationService operationService,
                      RulerToolOverlay.Factory overlayFactory)
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

        _overlay.OnRulerPlaced += RulerPlaced;
    }

    public void ToolEnd()
    {
        _toolOverlay.ActiveOverlay = null;
        _unitSnapOverlay.End();

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
