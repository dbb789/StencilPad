using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StencilPad.UI.Widgets;

public partial class FontNameComboBox : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(string), typeof(FontNameComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string? Value
    {
        get => (string?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IReadOnlyList<FontFamily> SortedFonts { get; } =
        Fonts.SystemFontFamilies
            .OrderBy(f => f.Source, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public FontNameComboBox()
    {
        InitializeComponent();
    }
}
