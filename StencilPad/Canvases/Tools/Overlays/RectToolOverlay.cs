using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Resolvers;
using StencilPad.Rendering;
using StencilPad.Spatial;
using StencilPad.Services;

namespace StencilPad.Canvases.Tools.Overlays;

public class RectToolOverlay<TSheetElement> : PolygonToolOverlayBase<TSheetElement>
    where TSheetElement : IPolygonSheetElement, new()
{
    public override TSheetElement Element => _element;

    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;
    private readonly IUnitSnapContext _unitSnapContext;
    private readonly ISettings _settings;
    private readonly IHintService _hintService;
    private readonly TSheetElement _element;
    private readonly Polygon _polygon;
    private readonly ISheetElementResolver? _resolver;
    private readonly ModelRenderer _renderer;

    private Unit2D? _initialPoint;
    private Unit2D _currentSnappedMousePosition;

    public RectToolOverlay(IViewport viewport,
                           IUnitSnap unitSnap,
                           ISettings settings,
                           IHintService hintService,
                           IResourceService resourceService)
    {
        _viewport = viewport;
        _unitSnap = unitSnap;
        _unitSnapContext = new DefaultUnitSnapContext(viewport);
        _settings = settings;
        _hintService = hintService;
        _element = new();

        _polygon = _element.PolygonSet.First();
        
        _resolver = ResolverFactory.Create(_element, _settings, resourceService);
        _renderer = new ModelRenderer(resourceService);

        _resolver?.Attach(_renderer);
        _renderer.RendererDirty += InvalidateVisual;
        
        _viewport.ViewportChanged += InvalidateVisual;
    }

    public override void Dispose()
    {
        ReleaseMouseCapture();

        _renderer.RendererDirty -= InvalidateVisual;
        _renderer.Dispose();
        _resolver?.Detach();
        
        _viewport.ViewportChanged -= InvalidateVisual;
        _hintService.ClearAll();
    }
    
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            if (_initialPoint is null)
            {
                _initialPoint = _currentSnappedMousePosition;
            }
            else
            {
                InvokePolygonCompleted(_polygon);
                _initialPoint = null;
                _polygon.Clear();
            }
        }

        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        _currentSnappedMousePosition = CurrentSnappedMouseOverPosition(e.GetPosition(this));
        
        if (_initialPoint is null)
        {
            _hintService.ClearHint();
            return;
        }

        while (_polygon.Vertices.Count < 4)
        {
            _polygon.AddVertex(new Vertex());
        }

        if (!_polygon.Closed)
        {
            _polygon.Close();
        }

        _polygon.Vertices[0] = new Vertex(new Unit2D(_initialPoint.Value.X, _initialPoint.Value.Y));
        _polygon.Vertices[1] = new Vertex(new Unit2D(_currentSnappedMousePosition.X, _initialPoint.Value.Y));
        _polygon.Vertices[2] = new Vertex(new Unit2D(_currentSnappedMousePosition.X, _currentSnappedMousePosition.Y));
        _polygon.Vertices[3] = new Vertex(new Unit2D(_initialPoint.Value.X, _currentSnappedMousePosition.Y));

        var size = Unit2D.Abs(_currentSnappedMousePosition - _initialPoint.Value);
        
        _hintService.SetHint($"Rectangle: {UnitUtil.FormatSuffixScaled(size.X, _settings.UnitSettings)} x {UnitUtil.FormatSuffixScaled(size.Y, _settings.UnitSettings)}");
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        if (_polygon.Vertices.Count == 0)
        {
            return;
        }

        dc.PushTransform(_viewport.MillimetersToPixelsTransform);

        _renderer.Render(dc);
        
        dc.Pop();
    }

    private Unit2D CurrentSnappedMouseOverPosition(Point mousePosition)
    {
        var unitPosition = _viewport.FromPoint(mousePosition);
        var snapPosition = _unitSnap.UnitSnap(unitPosition, _unitSnapContext);
        
        return snapPosition ?? unitPosition;
    }
}
