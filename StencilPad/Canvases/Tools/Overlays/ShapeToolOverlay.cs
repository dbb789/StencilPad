using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Canvases.Rendering;
using StencilPad.Canvases.Tools.Widgets;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class ShapeToolOverlay : Canvas, IDisposable
{
    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;
    private readonly Polygon _polygon;
    private readonly WidgetContainer<HandleWidget> _vertexWidgets;

    private Point _currentMousePosition;

    public event Action<Polygon>? OnPolygonCompleted;

    public ShapeToolOverlay(IViewport viewport, IUnitSnap unitSnap)
    {
        _viewport = viewport;
        _unitSnap = unitSnap;
        _polygon = new();
        _vertexWidgets = new(this);

        _polygon.GeometryChanged += RepositionWidgets;
        _viewport.ViewportChanged += RepositionWidgets;

        RepositionWidgets();
    }

    public void Dispose()
    {
        _polygon.GeometryChanged -= RepositionWidgets;
        _viewport.ViewportChanged -= RepositionWidgets;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (_polygon.Closed)
        {
            return;
        }

        _currentMousePosition = e.GetPosition(this);

        if (e.ClickCount == 1)
        {
            var unitPosition = _viewport.FromPoint(_currentMousePosition);

            if (_unitSnap.TryUnitSnap(unitPosition, null, out var snapped))
            {
                unitPosition = snapped;
            }

            if (!MouseOverExistingVertex(_currentMousePosition))
            {
                _polygon.AddVertex(new Vertex(unitPosition));
            }
        }
        else if (e.ClickCount == 2 && _polygon.Vertices.Count > 1)
        {
            if (MouseOverFirstVertex(_currentMousePosition))
            {
                _polygon.Close();
            }
            
            OnPolygonCompleted?.Invoke(_polygon);
            _polygon.Clear();
        }

        InvalidateVisual();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        _currentMousePosition = e.GetPosition(this);

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

        dc.PushTransform(_viewport.GetMillimetersToPixelsTransform());

        var shapePen = new Pen(Brushes.Black, 0.1);

        RendererUtil.Render(dc, shapePen, Brushes.Transparent, _polygon);

        if (!_polygon.Closed)
        {
            var lastPoint = _polygon.Vertices[^1].Position.Millimeters;
            var unitPosition = _viewport.FromPoint(_currentMousePosition);

            if (_unitSnap.TryUnitSnap(unitPosition, null, out var snapped))
            {
                unitPosition = snapped;
            }

            dc.DrawLine(shapePen, lastPoint, unitPosition.Millimeters);
        }

        dc.Pop();
    }

    private bool MouseOverExistingVertex(Point mousePosition)
    {
        foreach (var vertex in _polygon.Vertices)
        {
            if (MouseOverVertex(vertex, mousePosition))
            {
                return true;
            }
        }

        return false;
    }
    
    private bool MouseOverFirstVertex(Point mousePosition)
    {
        if (_polygon.Vertices.Count == 0)
        {
            return false;
        }

        return MouseOverVertex(_polygon.Vertices[0], mousePosition);
    }

    private bool MouseOverVertex(Vertex vertex, Point mousePosition)
    {
        const double hitRadius = 5.0;
        var hitRadiusSquared = hitRadius * hitRadius;

        var vertexScreenPosition = _viewport.ToPoint(vertex.Position);
        var distanceSquared = (vertexScreenPosition - mousePosition).LengthSquared;

        return (distanceSquared <= hitRadiusSquared);
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
