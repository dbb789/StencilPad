using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Tools.Widgets;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Models;
using StencilPad.Rendering;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class ShapeToolOverlay : Canvas, IDisposable
{
    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;
    private readonly IUnitSnapContext _unitSnapContext;
    private readonly Polygon _polygon;
    private readonly StreamGeometryWalker _walker;
    private readonly WidgetContainer<HandleWidget> _vertexWidgets;
    private readonly LockAxisState _lockAxisState;

    private Unit2D _currentSnappedMousePosition;
    
    public event Action<Polygon>? OnPolygonCompleted;

    public ShapeToolOverlay(IViewport viewport, IUnitSnap unitSnap)
    {
        _viewport = viewport;
        _unitSnap = unitSnap;
        _unitSnapContext = new DefaultUnitSnapContext(viewport);
        _polygon = new();
        _walker = new();
        _vertexWidgets = new(this);
        
        _lockAxisState = new();
        _polygon.GeometryChanged += GeometryChanged;
        _viewport.ViewportChanged += RepositionWidgets;

        RepositionWidgets();
    }

    public void Dispose()
    {
        ReleaseMouseCapture();

        _polygon.GeometryChanged -= GeometryChanged;
        _viewport.ViewportChanged -= RepositionWidgets;
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
                _polygon.AddVertex(new Vertex(_currentSnappedMousePosition));
            }
        }
        else if (e.ClickCount == 2 && _polygon.Vertices.Count > 1)
        {
            if (MouseOverFirstVertex())
            {
                _polygon.Close();
            }

            OnPolygonCompleted?.Invoke(_polygon);
            _polygon.Clear();
        }

        InvalidateVisual();

        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        _currentSnappedMousePosition = CurrentSnappedMouseOverPosition(e.GetPosition(this));
        
        if (_polygon.Vertices.Count == 0)
        {
            return;
        }

        InvalidateVisual();
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

        var geometry = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        using (var ctx = geometry.Open())
        {
            _walker.Context = ctx;
            _polygon.Resolver.Walk(_walker);
        }
        
        geometry.Freeze();
        
        var shapePen = new Pen(Brushes.Black, 0.1);

        dc.DrawGeometry(Brushes.Transparent, shapePen, geometry);
        
        if (!_polygon.Closed)
        {
            var lastPoint = _polygon.Vertices[^1].Position.Millimeters;

            dc.DrawLine(shapePen, lastPoint, _currentSnappedMousePosition.Millimeters);
        }

        dc.Pop();
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
        for (int i = 0; i < _polygon.Vertices.Count; ++i)
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
        const double hitRadius = 8.0;
        
        var hitRadiusSquared = hitRadius * hitRadius;

        var mousePixelPosition = _viewport.ToPoint(_currentSnappedMousePosition);

        var vertexScreenPosition = _viewport.ToPoint(vertex.Position);
        var distanceSquared = (vertexScreenPosition - mousePixelPosition).LengthSquared;

        return (distanceSquared <= hitRadiusSquared);
    }

    private void GeometryChanged(IPolygon polygon)
    {
        RepositionWidgets();
    }
    
    private void RepositionWidgets()
    {
        _vertexWidgets.Resize(_polygon.Vertices.Count);

        for (var i = 0; i < _polygon.Vertices.Count; i++)
        {
            var widget = _vertexWidgets[i];

            widget.Handle = Handle.DisplayOnly;
            widget.Selectable = false;
            widget.Draggable = false;
            widget.InvalidateVisual();

            var point = _viewport.ToPoint(_polygon.Vertices[i].Position);

            SetTop(widget, point.Y);
            SetLeft(widget, point.X);
        }
    }
}
