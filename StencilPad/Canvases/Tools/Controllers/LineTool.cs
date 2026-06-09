using System.Windows.Media;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers;

public abstract class LineTool : ITool
{
    protected abstract bool IsCurved { get; }
    
    private readonly Sheet _sheet;
    private readonly OverlayContainer _overlayContainer;
    private readonly IUnitSnapOverlay _unitSnapOverlay;
    private readonly IOperationService _operationService;
    private readonly Factory<PolygonToolOverlay<Shape>> _overlayFactory;
    private PolygonToolOverlay<Shape>? _overlay;

    protected LineTool(Sheet sheet,
                       OverlayContainer overlayContainer,
                       IUnitSnapOverlay unitSnapOverlay,
                       IOperationService operationService,
                       Factory<PolygonToolOverlay<Shape>> overlayFactory)
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
        _overlay.Element.LineColor = Color.FromArgb(128, 0, 0, 0);
        _overlay.IsCurved = IsCurved;
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
        _operationService.Push(new AddSheetElementOperation(_sheet.Id, new Shape(polygon)));
    }
}
