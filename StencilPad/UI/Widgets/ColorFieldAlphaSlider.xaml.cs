using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StencilPad.UI.Widgets;

public partial class ColorFieldAlphaSlider : ColorFieldSliderBase
{
    protected override double DisplayScale => 255;
    public static readonly DependencyProperty BaseColorProperty =
        DependencyProperty.Register(nameof(BaseColor), typeof(Color), typeof(ColorFieldAlphaSlider),
            new FrameworkPropertyMetadata(Colors.Black, OnBaseColorChanged));

    public Color BaseColor
    {
        get => (Color)GetValue(BaseColorProperty);
        set => SetValue(BaseColorProperty, value);
    }

    public ColorFieldAlphaSlider()
    {
        InitializeComponent();

        GradientRect.Fill = new LinearGradientBrush
        {
            StartPoint = new System.Windows.Point(0, 0.5),
            EndPoint = new System.Windows.Point(1, 0.5),
            GradientStops =
            {
                new GradientStop(Colors.Transparent, 0),
                new GradientStop(Colors.Black, 1)
            }
        };

        InitializeSlider(DragCanvas, Marker);
    }

    private static void OnBaseColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorFieldAlphaSlider slider)
        {
            slider.UpdateGradient();
        }
    }

    protected override void UpdateGradient()
    {
        var brush = (LinearGradientBrush)GradientRect.Fill;
        var c = BaseColor;

        brush.GradientStops[0].Color = Color.FromArgb(0, c.R, c.G, c.B);
        brush.GradientStops[1].Color = Color.FromArgb(255, c.R, c.G, c.B);
    }
}
