using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class MarkerPathTool : ITool
{
    public class Factory(Sheet Sheet,
                         ToolOverlay ToolOverlay,
                         IUnitSnapOverlay UnitSnapOverlay,
                         IOperationService OperationService,
                         ShapeToolOverlay.Factory OverlayFactory) : IToolFactory
    {
        public string IconResource => "MarkerPathTool";
        public string Tooltip => "Marker Path";

        public ITool Create(IToolButton button)
        {
            return new MarkerPathTool(Sheet, ToolOverlay, UnitSnapOverlay, OperationService, OverlayFactory);
        }
    }

    private readonly Sheet _sheet;
    private readonly ToolOverlay _toolOverlay;
    private readonly IUnitSnapOverlay _unitSnapOverlay;
    private readonly IOperationService _operationService;
    private readonly ShapeToolOverlay.Factory _overlayFactory;
    private ShapeToolOverlay? _overlay;

    private MarkerPathTool(Sheet sheet,
                           ToolOverlay toolOverlay,
                           IUnitSnapOverlay unitSnapOverlay,
                           IOperationService operationService,
                           ShapeToolOverlay.Factory overlayFactory)
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
                                         new MarkerPath(polygon)));
    }
}
