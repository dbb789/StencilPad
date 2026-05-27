using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StencilPad.Common;

namespace StencilPad.UI.Widgets;

public partial class ColorFieldSaturationSlider : ColorFieldSliderBase
{
    protected override double DisplayScale => 100;
    public static readonly DependencyProperty HueProperty =
        DependencyProperty.Register(nameof(Hue), typeof(double), typeof(ColorFieldSaturationSlider),
            new FrameworkPropertyMetadata(0.0, OnGradientChanged));

    public static readonly DependencyProperty BrightnessProperty =
        DependencyProperty.Register(nameof(Brightness), typeof(double), typeof(ColorFieldSaturationSlider),
            new FrameworkPropertyMetadata(1.0, OnGradientChanged));

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

        InitializeSlider(DragCanvas, Marker);
    }

    private static void OnGradientChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorFieldSaturationSlider slider)
        {
            slider.UpdateGradient();
        }
    }

    protected override void UpdateGradient()
    {
        var brush = (LinearGradientBrush)GradientRect.Fill;

        brush.GradientStops[0].Color = ColorUtil.HsvToRgb(Hue, 0, Brightness, 1);
        brush.GradientStops[1].Color = ColorUtil.HsvToRgb(Hue, 1, Brightness, 1);
    }
}
