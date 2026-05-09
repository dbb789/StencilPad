using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class RubberBandEventPanel : ContentControl, IRubberBand
{
    private readonly IViewport _viewport;
    private readonly RubberBandHandle _rubberBandHandle;

    public event Action<Rect?>? Updated;
    public event Action<UnitBounds>? BoundsSelected;
    public event Action<Unit2D>? PointSelected;

    public RubberBandEventPanel(IViewport viewport)
    {
        _viewport = viewport;
        _rubberBandHandle = new RubberBandHandle();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        var mousePosition = e.GetPosition(this);

        _rubberBandHandle.DragBegin(mousePosition);

        CaptureMouse();
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_rubberBandHandle.DragUpdate(e.GetPosition(this)))
        {
            if (_rubberBandHandle.IsDragging)
            {
                InvokeUpdated();
            }

            e.Handled = true;
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        ReleaseMouseCapture();

        if (_rubberBandHandle.IsDragging)
        {
            var rect = _rubberBandHandle.DragBounds;

            BoundsSelected?.Invoke(
                UnitBounds.FromMinMax(_viewport.FromPoint(rect.TopLeft),
                                      _viewport.FromPoint(rect.BottomRight)));
        }
        else
        {
            PointSelected?.Invoke(_viewport.FromPoint(e.GetPosition(this)));
        }
        
        _rubberBandHandle.DragEnd();

        InvokeUpdated();
        e.Handled = true;
    }

    private void InvokeUpdated()
    {
        Updated?.Invoke(GetDragRegion());
    }

    private Rect? GetDragRegion()
    {
        if (!_rubberBandHandle.IsDragging)
        {
            return null;
        }

        return _rubberBandHandle.DragBounds;
    }

    protected override void OnRender(DrawingContext dc)
    {
        dc.DrawRectangle(Brushes.Transparent,
                         null,
                         new Rect(0, 0, RenderSize.Width, RenderSize.Height));
    }
}
