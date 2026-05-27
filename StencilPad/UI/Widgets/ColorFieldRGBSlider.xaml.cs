using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StencilPad.UI.Widgets;

public partial class ColorFieldRGBSlider : ColorFieldSliderBase
{
    protected override double DisplayScale => 255;
    public static readonly DependencyProperty ChannelColorProperty =
        DependencyProperty.Register(nameof(ChannelColor), typeof(Color), typeof(ColorFieldRGBSlider),
            new FrameworkPropertyMetadata(Colors.Red, OnChannelColorChanged));

    public Color ChannelColor
    {
        get => (Color)GetValue(ChannelColorProperty);
        set => SetValue(ChannelColorProperty, value);
    }

    public ColorFieldRGBSlider()
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

    private static void OnChannelColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorFieldRGBSlider slider)
        {
            slider.UpdateGradient();
        }
    }

    protected override void UpdateGradient()
    {
        var brush = (LinearGradientBrush)GradientRect.Fill;
        brush.GradientStops[1].Color = ChannelColor;
    }
}
