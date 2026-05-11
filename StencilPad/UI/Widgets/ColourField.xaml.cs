using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StencilPad.UI.Widgets;

public partial class ColourField : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(Color), typeof(ColourField),
            new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty HexValueProperty =
        DependencyProperty.Register(nameof(HexValue), typeof(string), typeof(ColourField),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHexValueChanged));

    public static readonly DependencyProperty ColourBrushProperty =
        DependencyProperty.Register(nameof(ColourBrush), typeof(SolidColorBrush), typeof(ColourField),
            new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Black)));

    public Color Value
    {
        get => (Color)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string HexValue
    {
        get => (string)GetValue(HexValueProperty);
        set => SetValue(HexValueProperty, value);
    }

    public SolidColorBrush ColourBrush
    {
        get => (SolidColorBrush)GetValue(ColourBrushProperty);
        set => SetValue(ColourBrushProperty, value);
    }

    public ColourField()
    {
        InitializeComponent();
        UpdateFromValue();
    }

    private bool _isUpdating;

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ColourField field || field._isUpdating)
        {
            return;
        }

        field._isUpdating = true;

        try
        {
            field.UpdateFromValue();
        }
        finally
        {
            field._isUpdating = false;
        }
    }

    private static void OnHexValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ColourField field || field._isUpdating)
        {
            return;
        }

        field._isUpdating = true;

        try
        {
            if (TryParseHex(field.HexValue, out var colour))
            {
                field.Value = colour;
                field.ColourBrush = new SolidColorBrush(colour);
            }
        }
        finally
        {
            field._isUpdating = false;
        }
    }

    private void UpdateFromValue()
    {
        HexValue = FormatHex(Value);
        ColourBrush = new SolidColorBrush(Value);
    }

    private static string FormatHex(Color colour)
    {
        return $"#{colour.A:X2}{colour.R:X2}{colour.G:X2}{colour.B:X2}";
    }

    private static bool TryParseHex(string text, out Color colour)
    {
        colour = Colors.Black;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var s = text.TrimStart('#');

        try
        {
            if (s.Length == 6 &&
                byte.TryParse(s[0..2], System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                byte.TryParse(s[2..4], System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                byte.TryParse(s[4..6], System.Globalization.NumberStyles.HexNumber, null, out var b))
            {
                colour = Color.FromArgb(255, r, g, b);
                return true;
            }

            if (s.Length == 8 &&
                byte.TryParse(s[0..2], System.Globalization.NumberStyles.HexNumber, null, out var a2) &&
                byte.TryParse(s[2..4], System.Globalization.NumberStyles.HexNumber, null, out var r2) &&
                byte.TryParse(s[4..6], System.Globalization.NumberStyles.HexNumber, null, out var g2) &&
                byte.TryParse(s[6..8], System.Globalization.NumberStyles.HexNumber, null, out var b2))
            {
                colour = Color.FromArgb(a2, r2, g2, b2);
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}
