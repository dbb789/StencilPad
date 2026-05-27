using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StencilPad.Common;

namespace StencilPad.UI.Widgets;

public partial class ColorFieldBrightnessSlider : ColorFieldSliderBase
{
    protected override double DisplayScale => 100;
    public static readonly DependencyProperty HueProperty =
        DependencyProperty.Register(nameof(Hue), typeof(double), typeof(ColorFieldBrightnessSlider),
            new FrameworkPropertyMetadata(0.0, OnGradientChanged));

    public static readonly DependencyProperty SaturationProperty =
        DependencyProperty.Register(nameof(Saturation), typeof(double), typeof(ColorFieldBrightnessSlider),
            new FrameworkPropertyMetadata(1.0, OnGradientChanged));

    public double Hue
    {
        get => (double)GetValue(HueProperty);
        set => SetValue(HueProperty, value);
    }

    public double Saturation
    {
        get => (double)GetValue(SaturationProperty);
        set => SetValue(SaturationProperty, value);
    }

    public ColorFieldBrightnessSlider()
    {
        InitializeComponent();

        GradientRect.Fill = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0.5),
            EndPoint = new Point(1, 0.5),
            GradientStops =
            {
                new GradientStop(Colors.Black, 0),
                new GradientStop(Colors.Red, 1)
            }
        };

        InitializeSlider(DragCanvas, Marker);
    }

    private static void OnGradientChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorFieldBrightnessSlider slider)
        {
            slider.UpdateGradient();
        }
    }

    protected override void UpdateGradient()
    {
        var brush = (LinearGradientBrush)GradientRect.Fill;

        brush.GradientStops[0].Color = Colors.Black;
        brush.GradientStops[1].Color = ColorUtil.HsvToRgb(Hue, Saturation, 1, 1);
    }
}
