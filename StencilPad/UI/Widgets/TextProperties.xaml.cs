using System.Windows;
using System.Windows.Controls;

namespace StencilPad.UI.Widgets;

public partial class TextProperties : UserControl
{
    public static readonly DependencyProperty TextFontNameProperty =
        DependencyProperty.Register(nameof(TextFontName), typeof(string), typeof(TextProperties),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty TextFontSizeProperty =
        DependencyProperty.Register(nameof(TextFontSize), typeof(double), typeof(TextProperties),
            new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string? TextFontName
    {
        get => (string?)GetValue(TextFontNameProperty);
        set => SetValue(TextFontNameProperty, value);
    }

    public double TextFontSize
    {
        get => (double)GetValue(TextFontSizeProperty);
        set => SetValue(TextFontSizeProperty, value);
    }

    public TextProperties()
    {
        InitializeComponent();
    }
}
