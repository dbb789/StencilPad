using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class RubberBandEventPanel : ContentControl, IRubberBand
{
    public RubberBandRenderPanel? RenderPanel;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;

                if (!_isActive)
                {
                    _rubberBandHandle.DragEnd();
                }
                
                UpdatePanel();
            }
        }
    }
    
    private readonly IViewport _viewport;
    private readonly RubberBandHandle _rubberBandHandle;
    private bool _isActive;
    
    public event Action<UnitBounds>? BoundsSelected;
    public event Action<Unit2D>? PointSelected;

    public RubberBandEventPanel(IViewport viewport)
    {
        _viewport = viewport;
        _rubberBandHandle = new RubberBandHandle();
        _isActive = false;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (!_isActive)
        {
            return;
        }
        
        var mousePosition = e.GetPosition(this);

        _rubberBandHandle.DragBegin(mousePosition);

        CaptureMouse();
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!_isActive)
        {
            return;
        }

        if (_rubberBandHandle.DragUpdate(e.GetPosition(this)))
        {
            if (_rubberBandHandle.IsDragging)
            {
                UpdatePanel();
            }

            e.Handled = true;
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!_isActive)
        {
            return;
        }

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

        UpdatePanel();
        e.Handled = true;
    }

    private void UpdatePanel()
    {
        RenderPanel?.Updated(GetDragRegion());
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
