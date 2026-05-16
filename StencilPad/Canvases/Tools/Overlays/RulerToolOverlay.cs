using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class RulerToolOverlay : Canvas, IDisposable
{
    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;

    private Unit2D? _start;
    private Point _currentMousePosition;

    public event Action<Unit2D, Unit2D>? OnRulerPlaced;

    public RulerToolOverlay(IViewport viewport, IUnitSnap unitSnap)
    {
        _viewport = viewport;
        _unitSnap = unitSnap;

        _viewport.ViewportChanged += OnViewportChanged;
    }

    public void Dispose()
    {
        _viewport.ViewportChanged -= OnViewportChanged;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        _currentMousePosition = e.GetPosition(this);

        var unitPosition = _viewport.FromPoint(_currentMousePosition);
        var snapPosition = _unitSnap.UnitSnap(unitPosition, EmptyUnitSnapContext.Instance);
        
        if (snapPosition.HasValue)
        {
            unitPosition = snapPosition.Value;
        }

        if (_start is null)
        {
            _start = unitPosition;
        }
        else
        {
            OnRulerPlaced?.Invoke(_start.Value, unitPosition);
            _start = null;
        }

        InvalidateVisual();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        _currentMousePosition = e.GetPosition(this);

        if (_start is not null)
        {
            InvalidateVisual();
        }
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        if (_start is null)
        {
            return;
        }

        dc.PushTransform(_viewport.GetMillimetersToPixelsTransform());

        var pen = new Pen(Brushes.Gray, 0.2) { DashStyle = DashStyles.Dash };
        var startPoint = _start.Value.Millimeters;

        var unitPosition = _viewport.FromPoint(_currentMousePosition);
        var snapPosition = _unitSnap.UnitSnap(unitPosition, EmptyUnitSnapContext.Instance);
        
        if (snapPosition.HasValue)
        {
            unitPosition = snapPosition.Value;
        }

        var endPoint = unitPosition.Millimeters;
        
        dc.DrawLine(pen, startPoint, endPoint);

        dc.Pop();
    }

    private void OnViewportChanged()
    {
        InvalidateVisual();
    }
}
