using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StencilPad.UI.Widgets;

public partial class ColorFieldHueSlider : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(ColorFieldHueSlider),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private bool _dragging;

    public event EventHandler? ValueChanged;

    public ColorFieldHueSlider()
    {
        InitializeComponent();

        Loaded += (_, _) => UpdateMarkerPosition();
        SizeChanged += (_, _) => UpdateMarkerPosition();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ColorFieldHueSlider slider)
        {
            return;
        }

        slider.UpdateMarkerPosition();
    }

    private void DragCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _dragging = true;
        DragCanvas.CaptureMouse();
        SetValueFromPoint(e.GetPosition(DragCanvas));
    }

    private void DragCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_dragging)
        {
            SetValueFromPoint(e.GetPosition(DragCanvas));
        }
    }

    private void DragCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_dragging)
        {
            return;
        }

        _dragging = false;
        DragCanvas.ReleaseMouseCapture();
    }

    private void SetValueFromPoint(Point p)
    {
        Value = Math.Clamp(p.X / DragCanvas.ActualWidth, 0, 1) * 360;
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateMarkerPosition()
    {
        if (!IsLoaded)
        {
            return;
        }

        var x = Value / 360.0 * DragCanvas.ActualWidth;

        Canvas.SetLeft(Marker, x - Marker.Width / 2);
        Canvas.SetTop(Marker, (DragCanvas.ActualHeight - Marker.Height) / 2);
    }
}
