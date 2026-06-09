using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Widgets;
using StencilPad.Models;
using StencilPad.Models.Resolvers;
using StencilPad.Rendering;
using StencilPad.Spatial;
using StencilPad.Services;

namespace StencilPad.Canvases.Tools.Overlays;

public class CircleToolOverlay<TSheetElement> : PolygonToolOverlayBase<TSheetElement>
    where TSheetElement : IPolygonSheetElement, new()
{
    public override TSheetElement Element => _element;

    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;
    private readonly IUnitSnapContext _unitSnapContext;
    private readonly TSheetElement _element;
    private readonly Polygon _polygon;
    private readonly IModelResolver? _resolver;
    private readonly ModelRenderer _renderer;

    private Unit2D? _initialPoint;
    private Unit2D _currentSnappedMousePosition;

    public CircleToolOverlay(IViewport viewport,
                             IUnitSnap unitSnap,
                             IResourceService resourceService)
    {
        _viewport = viewport;
        _unitSnap = unitSnap;
        _unitSnapContext = new DefaultUnitSnapContext(viewport);
        _element = new();

        _polygon = _element.PolygonSet.First();
        
        _resolver = ResolverFactory.Create(_element, resourceService);
        _renderer = new ModelRenderer(resourceService);

        _resolver?.Attach(_renderer);
        _renderer.RendererDirty += InvalidateVisual;
        
        _viewport.ViewportChanged += InvalidateVisual;
    }

    public override void Dispose()
    {
        ReleaseMouseCapture();

        _renderer.RendererDirty -= InvalidateVisual;
        _resolver?.Detach();

        _viewport.ViewportChanged -= InvalidateVisual;
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
            return;
        }

        while (_polygon.Vertices.Count < 4)
        {
            _polygon.AddVertex(CreateCircleVertex(Unit2D.Zero));
        }

        if (!_polygon.Closed)
        {
            _polygon.Close();
        }

        var offset = Unit2D.Abs(_currentSnappedMousePosition - _initialPoint.Value);

        _polygon.Vertices[0] = CreateCircleVertex(_initialPoint.Value - offset);
        _polygon.Vertices[1] = CreateCircleVertex(new Unit2D(_initialPoint.Value.X - offset.X,
                                                             _initialPoint.Value.Y + offset.Y));
        _polygon.Vertices[2] = CreateCircleVertex(_initialPoint.Value + offset);
        _polygon.Vertices[3] = CreateCircleVertex(new Unit2D(_initialPoint.Value.X + offset.X,
                                                             _initialPoint.Value.Y - offset.Y));
    }

    private Vertex CreateCircleVertex(Unit2D position)
    {
        return new Vertex
        {
            Position = position,
            CornerType = CornerType.Rounded,
            CornerSize = CornerSize.FromProportion(1)
        };
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
