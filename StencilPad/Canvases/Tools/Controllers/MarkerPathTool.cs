using System.Windows.Media;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public class MarkerPathTool : ITool
{
    public class Factory(Sheet Sheet,
                         OverlayContainer OverlayContainer,
                         ISettings Settings,
                         IUnitSnapOverlay UnitSnapOverlay,
                         IOperationService OperationService,
                         Factory<LineToolOverlay<MarkerPath>> OverlayFactory) : IToolFactory
    {
        public string IconResource => "MarkerPathTool";
        public string Tooltip => "Marker Path";

        public ITool Create(IToolButton button)
        {
            return new MarkerPathTool(Sheet,
                                      OverlayContainer,
                                      Settings,
                                      UnitSnapOverlay,
                                      OperationService,
                                      OverlayFactory);
        }
    }

    private readonly Sheet _sheet;
    private readonly OverlayContainer _overlayContainer;
    private readonly ISettings _settings;
    private readonly IUnitSnapOverlay _unitSnapOverlay;
    private readonly IOperationService _operationService;
    private readonly Factory<LineToolOverlay<MarkerPath>> _overlayFactory;
    private LineToolOverlay<MarkerPath>? _overlay;

    private MarkerPathTool(Sheet sheet,
                           OverlayContainer overlayContainer,
                           ISettings settings,
                           IUnitSnapOverlay unitSnapOverlay,
                           IOperationService operationService,
                           Factory<LineToolOverlay<MarkerPath>> overlayFactory)
    {
        _sheet = sheet;
        _overlayContainer = overlayContainer;
        _settings = settings;
        _unitSnapOverlay = unitSnapOverlay;
        _operationService = operationService;
        _overlayFactory = overlayFactory;
    }

    public void Dispose()
    { }

    public void ToolBegin()
    {
        _overlay = _overlayFactory.Create();
        _overlay.Element.LineColor = Color.FromArgb(128, 0, 0, 0);
        _overlay.Element.MarkerColor = Color.FromArgb(128, 0, 0, 0);

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
        var element = new MarkerPath(polygon);
        
        _settings.GetElementStyle(element);

        _operationService.Push( new AddSheetElementOperation(_sheet.Id, element));
    }
}
