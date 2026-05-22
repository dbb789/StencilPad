using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Common;

namespace StencilPad.UI.Widgets;

public partial class ColorFieldSaturationSlider : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(ColorFieldSaturationSlider),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty HueProperty =
        DependencyProperty.Register(nameof(Hue), typeof(double), typeof(ColorFieldSaturationSlider),
            new FrameworkPropertyMetadata(0.0, OnGradientChanged));

    public static readonly DependencyProperty BrightnessProperty =
        DependencyProperty.Register(nameof(Brightness), typeof(double), typeof(ColorFieldSaturationSlider),
            new FrameworkPropertyMetadata(1.0, OnGradientChanged));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Hue
    {
        get => (double)GetValue(HueProperty);
        set => SetValue(HueProperty, value);
    }

    public double Brightness
    {
        get => (double)GetValue(BrightnessProperty);
        set => SetValue(BrightnessProperty, value);
    }

    private bool _dragging;

    public event EventHandler? ValueChanged;

    public ColorFieldSaturationSlider()
    {
        InitializeComponent();

        GradientRect.Fill = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0.5),
            EndPoint = new Point(1, 0.5),
            GradientStops =
            {
                new GradientStop(Colors.Gray, 0),
                new GradientStop(Colors.Red, 1)
            }
        };

        Loaded += (_, _) =>
        {
            UpdateGradient();
            UpdateMarkerPosition();
        };
        SizeChanged += (_, _) => UpdateMarkerPosition();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ColorFieldSaturationSlider slider)
        {
            return;
        }

        slider.UpdateMarkerPosition();
    }

    private static void OnGradientChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ColorFieldSaturationSlider slider)
        {
            return;
        }

        slider.UpdateGradient();
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
        Value = Math.Clamp(p.X / DragCanvas.ActualWidth, 0, 1);
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateGradient()
    {
        var brush = (LinearGradientBrush)GradientRect.Fill;

        brush.GradientStops[0].Color = ColorUtil.HsvToRgb(Hue, 0, Brightness, 1);
        brush.GradientStops[1].Color = ColorUtil.HsvToRgb(Hue, 1, Brightness, 1);
    }

    private void UpdateMarkerPosition()
    {
        if (!IsLoaded)
        {
            return;
        }

        var x = Value * DragCanvas.ActualWidth;

        Canvas.SetLeft(Marker, x - Marker.Width / 2);
        Canvas.SetTop(Marker, (DragCanvas.ActualHeight - Marker.Height) / 2);
    }
}
