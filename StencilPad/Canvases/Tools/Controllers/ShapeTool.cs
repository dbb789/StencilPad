using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class ShapeTool : ITool
{
    public class Factory(Sheet Sheet,
                         OverlayContainer OverlayContainer,
                         IUnitSnapOverlay UnitSnapOverlay,
                         IOperationService OperationService,
                         Factory<ShapeToolOverlay> OverlayFactory) : IToolFactory
    {
        public string IconResource => "StraightLineTool";
        public string Tooltip => "Shape";

        public ITool Create(IToolButton button)
        {
            return new ShapeTool(Sheet, OverlayContainer, UnitSnapOverlay, OperationService, OverlayFactory);
        }
    }

    private readonly Sheet _sheet;
    private readonly OverlayContainer _overlayContainer;
    private readonly IUnitSnapOverlay _unitSnapOverlay;
    private readonly IOperationService _operationService;
    private readonly Factory<ShapeToolOverlay> _overlayFactory;
    private ShapeToolOverlay? _overlay;

    private ShapeTool(Sheet sheet,
                      OverlayContainer overlayContainer,
                      IUnitSnapOverlay unitSnapOverlay,
                      IOperationService operationService,
                      Factory<ShapeToolOverlay> overlayFactory)
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

        _overlay.OnPolygonCompleted += PolygonCompleted;
    }

    public void ToolEnd()
    {
        _overlayContainer.ActiveOverlay = null;
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
