using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace StencilPad.UI.Widgets;

public abstract class ColorFieldSliderBase : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(ColorFieldSliderBase),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private Canvas? _dragCanvas;
    private Rectangle? _marker;
    private bool _dragging;

    public event EventHandler? ValueChanged;

    protected void InitializeSlider(Canvas dragCanvas, Rectangle marker)
    {
        _dragCanvas = dragCanvas;
        _marker = marker;

        dragCanvas.MouseDown += DragCanvas_MouseDown;
        dragCanvas.MouseMove += DragCanvas_MouseMove;
        dragCanvas.MouseUp += DragCanvas_MouseUp;

        Loaded += (_, _) =>
        {
            UpdateGradient();
            UpdateMarkerPosition();
        };
        SizeChanged += (_, _) => UpdateMarkerPosition();
    }

    protected abstract void UpdateGradient();

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorFieldSliderBase slider)
        {
            slider.UpdateMarkerPosition();
        }
    }

    private void DragCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _dragging = true;
        _dragCanvas!.CaptureMouse();
        SetValueFromPoint(e.GetPosition(_dragCanvas));
    }

    private void DragCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_dragging)
        {
            SetValueFromPoint(e.GetPosition(_dragCanvas));
        }
    }

    private void DragCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_dragging)
        {
            return;
        }

        _dragging = false;
        _dragCanvas!.ReleaseMouseCapture();
    }

    private void SetValueFromPoint(Point p)
    {
        var halfMarker = _marker!.Width / 2;
        var usable = _dragCanvas!.ActualWidth - _marker.Width;
        Value = Math.Clamp((p.X - halfMarker) / usable, 0, 1);
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateMarkerPosition()
    {
        if (!IsLoaded || _dragCanvas is null || _marker is null)
        {
            return;
        }

        var halfMarker = _marker.Width / 2;
        var usable = _dragCanvas.ActualWidth - _marker.Width;
        var x = halfMarker + Value * usable;

        Canvas.SetLeft(_marker, x - halfMarker);
        Canvas.SetTop(_marker, (_dragCanvas.ActualHeight - _marker.Height) / 2);
    }
}
