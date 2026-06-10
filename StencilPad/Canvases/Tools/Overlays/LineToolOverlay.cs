using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Resolvers;
using StencilPad.Rendering;
using StencilPad.Spatial;
using StencilPad.Services;

namespace StencilPad.Canvases.Tools.Overlays;

public class LineToolOverlay<TSheetElement> : PolygonToolOverlayBase<TSheetElement>
    where TSheetElement : IPolygonSheetElement, new()
{
    public override TSheetElement Element => _element;
    public bool IsCurved { get; set; } = false;

    private readonly ISettings _settings;
    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;
    private readonly IUnitSnapContext _unitSnapContext;
    private readonly TSheetElement _element;
    private readonly Polygon _polygon;
    private readonly IModelResolver? _resolver;
    private readonly ModelRenderer _renderer;
    private readonly LockAxisState _lockAxisState;

    private Unit2D _currentSnappedMousePosition;
    private double _handleSize;
    private Brush _moveBrush = null!;

    public LineToolOverlay(ISettings settings,
                           IViewport viewport,
                           IUnitSnap unitSnap,
                           IResourceService resourceService)
    {
        _settings = settings;
        _viewport = viewport;
        _unitSnap = unitSnap;
        _unitSnapContext = new DefaultUnitSnapContext(viewport);
        _element = new();

        _polygon = _element.PolygonSet.First();
        
        AddVertexAtMousePosition();

        _resolver = ResolverFactory.Create(_element, resourceService);
        _renderer = new ModelRenderer(resourceService);

        _resolver?.Attach(_renderer);
        _renderer.RendererDirty += InvalidateVisual;
        
        _lockAxisState = new();
        _viewport.ViewportChanged += InvalidateVisual;

        BuildPens();
        
        _settings.Changed += SettingsChanged;
    }

    public override void Dispose()
    {
        ReleaseMouseCapture();

        _settings.Changed -= SettingsChanged;
        _renderer.RendererDirty -= InvalidateVisual;
        _resolver?.Detach();

        _viewport.ViewportChanged -= InvalidateVisual;
    }
    
    private void BuildPens()
    {
        var moveHandleColor = _settings.MoveHandleColor;
        var adjustHandleColor = _settings.AdjustHandleColor;
        var selectionColor = _settings.SelectionColor;
        var gridLineColor = _settings.GridLineColor;
        
        _moveBrush = new SolidColorBrush(ColorUtil.WithAlpha(moveHandleColor, 128));
        _moveBrush.Freeze();

        _handleSize = _settings.HandleSizePx;
    }
    
    private void SettingsChanged()
    {
        BuildPens();
        InvalidateVisual();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (_polygon.Closed)
        {
            return;
        }

        if (e.ClickCount == 1)
        {
            if (!MouseOverExistingVertex())
            {
                AddVertexAtMousePosition();
            }
        }
        else if (e.ClickCount == 2 && _polygon.Vertices.Count > 2)
        {
            _polygon.DeleteVertex(_polygon.Vertices.Count - 1);
            
            if (MouseOverFirstVertex())
            {
                _polygon.Close();
                
                if (IsCurved)
                {
                    var edge = _polygon.Edges[^1];
                    
                    _polygon.Edges[^1] = edge with { Type = EdgeType.Bezier };
                    _polygon.CalculateControlPoints(_polygon.Edges.Count - 1, false);
                }
            }

            InvokePolygonCompleted(_polygon);
            _polygon.Clear();
            AddVertexAtMousePosition();
        }

        e.Handled = true;
    }

    private void AddVertexAtMousePosition()
    {
        _polygon.AddVertex(new Vertex { Position = _currentSnappedMousePosition });

        if (IsCurved &&_polygon.Vertices.Count > 1)
        {
            var edge = _polygon.Edges[^1];
            
            _polygon.Edges[^1] = edge with { Type = EdgeType.Bezier };
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        _currentSnappedMousePosition = CurrentSnappedMouseOverPosition(e.GetPosition(this));
        
        if (_polygon.Vertices.Count == 0)
        {
            return;
        }
        
        var vertex = _polygon.Vertices[^1];
        
        _polygon.Vertices[^1] = vertex with { Position = _currentSnappedMousePosition };

        if (IsCurved && _polygon.Edges.Count > 0)
        {
            _polygon.CalculateControlPoints(_polygon.Edges.Count - 1, false);
        }
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

        for (int i = 0; i < _polygon.Vertices.Count; ++i)
        {
            var point = _viewport.ToPoint(_polygon.Vertices[i].Position);

            dc.DrawRectangle(_moveBrush,
                             null,
                             new Rect(point.X - (_handleSize / 2),
                                      point.Y - (_handleSize / 2),
                                      _handleSize,
                                      _handleSize));
        }
    }

    private Unit2D CurrentSnappedMouseOverPosition(Point mousePosition)
    {
        var unitPosition = _viewport.FromPoint(mousePosition);
        var snapPosition = _unitSnap.UnitSnap(unitPosition, _unitSnapContext);
        
        if (snapPosition.HasValue)
        {
            unitPosition = snapPosition.Value;
        }
        
        if (_polygon.Vertices.Count > 0)
        {
            unitPosition = _lockAxisState.OnDragMove(ModifierUtil.IsLockToAxis(),
                                                     _viewport.FromPixels(12),
                                                     _polygon.Vertices[^1].Position,
                                                     unitPosition);
        }

        return unitPosition;
    }

    private bool MouseOverExistingVertex()
    {
        // NOTE: Ignore the last vertex since it's always the one already under
        // the mouse cursor.
        for (int i = 0; i < _polygon.Vertices.Count - 1; ++i)
        {
            var vertex = _polygon.Vertices[i];

            if (MouseOverVertex(vertex))
            {
                return true;
            }
        }

        return false;
    }
    
    private bool MouseOverFirstVertex()
    {
        if (_polygon.Vertices.Count == 0)
        {
            return false;
        }

        return MouseOverVertex(_polygon.Vertices[0]);
    }

    private bool MouseOverVertex(Vertex vertex)
    {
        double hitRadius = _handleSize * 1.25;
        var hitRadiusSquared = hitRadius * hitRadius;
        var mousePixelPosition = _viewport.ToPoint(_currentSnappedMousePosition);
        
        var vertexScreenPosition = _viewport.ToPoint(vertex.Position);
        var distanceSquared = (vertexScreenPosition - mousePixelPosition).LengthSquared;

        return (distanceSquared <= hitRadiusSquared);
    }
}
