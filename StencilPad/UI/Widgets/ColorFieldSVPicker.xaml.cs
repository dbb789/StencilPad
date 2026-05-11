using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StencilPad.UI.Widgets;

public partial class ColorFieldSVPicker : UserControl
{
    public static readonly DependencyProperty SaturationProperty =
        DependencyProperty.Register(nameof(Saturation), typeof(double), typeof(ColorFieldSVPicker),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPositionChanged));

    public static readonly DependencyProperty BrightnessProperty =
        DependencyProperty.Register(nameof(Brightness), typeof(double), typeof(ColorFieldSVPicker),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPositionChanged));

    public static readonly DependencyProperty HueColorProperty =
        DependencyProperty.Register(nameof(HueColor), typeof(Color), typeof(ColorFieldSVPicker),
            new FrameworkPropertyMetadata(Colors.Red, OnHueColorChanged));

    public double Saturation
    {
        get => (double)GetValue(SaturationProperty);
        set => SetValue(SaturationProperty, value);
    }

    public double Brightness
    {
        get => (double)GetValue(BrightnessProperty);
        set => SetValue(BrightnessProperty, value);
    }

    public Color HueColor
    {
        get => (Color)GetValue(HueColorProperty);
        set => SetValue(HueColorProperty, value);
    }

    private bool _dragging;

    public event EventHandler? ValueChanged;

    public ColorFieldSVPicker()
    {
        InitializeComponent();

        Loaded += (_, _) => UpdateMarkerPosition();
        SizeChanged += (_, _) => UpdateMarkerPosition();
    }

    private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ColorFieldSVPicker picker)
        {
            return;
        }

        picker.UpdateMarkerPosition();
    }

    private static void OnHueColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ColorFieldSVPicker picker)
        {
            return;
        }

        picker.UpdateGradient();
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
        Saturation = Math.Clamp(p.X / DragCanvas.ActualWidth, 0, 1);
        Brightness = Math.Clamp(1 - p.Y / DragCanvas.ActualHeight, 0, 1);
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateGradient()
    {
        var brush = (LinearGradientBrush)SaturationRect.Fill;
        brush.GradientStops[1].Color = HueColor;
    }

    private void UpdateMarkerPosition()
    {
        if (!IsLoaded)
        {
            return;
        }

        var x = Saturation * DragCanvas.ActualWidth;
        var y = (1 - Brightness) * DragCanvas.ActualHeight;

        Canvas.SetLeft(MarkerOuter, x - MarkerOuter.Width / 2);
        Canvas.SetTop(MarkerOuter, y - MarkerOuter.Height / 2);
        Canvas.SetLeft(Marker, x - Marker.Width / 2);
        Canvas.SetTop(Marker, y - Marker.Height / 2);
    }
}
