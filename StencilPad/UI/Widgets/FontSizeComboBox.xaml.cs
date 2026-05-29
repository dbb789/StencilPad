using System.Windows;
using System.Windows.Controls;

namespace StencilPad.UI.Widgets;

public partial class FontSizeComboBox : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(FontSizeComboBox),
            new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public FontSizeComboBox()
    {
        InitializeComponent();
    }
}
