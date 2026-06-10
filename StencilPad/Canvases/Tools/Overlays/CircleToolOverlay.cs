using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Common;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Common;
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
    private readonly ISettings _settings;
    private readonly IHintService _hintService;
    private readonly TSheetElement _element;
    private readonly Polygon _polygon;
    private readonly IModelResolver? _resolver;
    private readonly ModelRenderer _renderer;

    private Unit2D? _initialPoint;
    private Unit2D _currentSnappedMousePosition;

    public CircleToolOverlay(IViewport viewport,
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
            _hintService.ClearHint();
            return;
        }

        // Alternative implementation that creates a circle out of curved
        // corners, and a pill when the width and height are different. The
        // circle is created by placing 4 vertices at the cardinal points, and
        // setting the corner type to rounded with a corner size equal to the
        // distance from the vertex to the center.
        //
        // We might want to enable this as a setting.
        //
        // (_polygon.Vertices.Count < 4) {
        // _polygon.AddVertex(CreateCircleVertex(Unit2D.Zero)); }

        // if (!_polygon.Closed)
        // {
        //     _polygon.Close();
        // }

        // var offset = Unit2D.Abs(_currentSnappedMousePosition - _initialPoint.Value);

        // _polygon.Vertices[0] = CreateCircleVertex(_initialPoint.Value - offset);
        // _polygon.Vertices[1] = CreateCircleVertex(new Unit2D(_initialPoint.Value.X - offset.X,
        //                                                      _initialPoint.Value.Y + offset.Y));
        // _polygon.Vertices[2] = CreateCircleVertex(_initialPoint.Value + offset);
        // _polygon.Vertices[3] = CreateCircleVertex(new Unit2D(_initialPoint.Value.X + offset.X,
        //                                                      _initialPoint.Value.Y - offset.Y));
        //
        // Vertex CreateCircleVertex(Unit2D position)
        // {
        //     return new Vertex
        //     {
        //         Position = position,
        //         CornerType = CornerType.Rounded,
        //         CornerSize = CornerSize.FromProportion(1)
        //     };
        // }

        while (_polygon.Vertices.Count < 4)
        {
            _polygon.AddVertex(new Vertex());
        }

        if (!_polygon.Closed)
        {
            _polygon.Close();
        }

        var size = Unit2D.Abs(_currentSnappedMousePosition - _initialPoint.Value);

        if (ModifierUtil.IsLockAspect())
        {
            var maxSize = Unit.Max(size.X, size.Y);
            
            size = new Unit2D(maxSize, maxSize);
        }

        _polygon.Vertices[0] = new Vertex(new Unit2D(_initialPoint.Value.X, _initialPoint.Value.Y - size.Y));
        _polygon.Vertices[1] = new Vertex(new Unit2D(_initialPoint.Value.X + size.X, _initialPoint.Value.Y));
        _polygon.Vertices[2] = new Vertex(new Unit2D(_initialPoint.Value.X, _initialPoint.Value.Y + size.Y));
        _polygon.Vertices[3] = new Vertex(new Unit2D(_initialPoint.Value.X - size.X, _initialPoint.Value.Y));

        _polygon.Edges[0] = new Edge
        {
            Type = EdgeType.Bezier,
            ControlBeginOffset = new Unit2D(size.X * MathUtil.Kappa, Unit.Zero),
            ControlEndOffset = new Unit2D(Unit.Zero, -size.Y * MathUtil.Kappa)
        };
        
        _polygon.Edges[1] = new Edge
        {
            Type = EdgeType.Bezier,
            ControlBeginOffset = new Unit2D(Unit.Zero, size.Y * MathUtil.Kappa),
            ControlEndOffset = new Unit2D(size.X * MathUtil.Kappa, Unit.Zero)
        };
        
        _polygon.Edges[2] = new Edge
        {
            Type = EdgeType.Bezier,
            ControlBeginOffset = new Unit2D(-size.X * MathUtil.Kappa, Unit.Zero),
            ControlEndOffset = new Unit2D(Unit.Zero, size.Y * MathUtil.Kappa)
        };

        _polygon.Edges[3] = new Edge
        {
            Type = EdgeType.Bezier,
            ControlBeginOffset = new Unit2D(Unit.Zero, -size.Y * MathUtil.Kappa),
            ControlEndOffset = new Unit2D(-size.X * MathUtil.Kappa, Unit.Zero)
        };

        _hintService.SetHint($"{UnitUtil.FormatSuffix(size.X, _settings.UnitSettings)} x {UnitUtil.FormatSuffix(size.Y, _settings.UnitSettings)}");
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
