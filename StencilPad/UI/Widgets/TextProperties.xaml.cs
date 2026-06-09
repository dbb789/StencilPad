using System.Windows;
using System.Windows.Controls;

namespace StencilPad.UI.Widgets;

public partial class TextProperties : UserControl
{
    public static readonly DependencyProperty FontNameProperty =
        DependencyProperty.Register(nameof(FontName), typeof(string), typeof(TextProperties),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(TextProperties),
            new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string? FontName
    {
        get => (string?)GetValue(FontNameProperty);
        set => SetValue(FontNameProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public TextProperties()
    {
        InitializeComponent();
    }
}
