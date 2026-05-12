using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StencilPad.Common;

namespace StencilPad.UI.Widgets;

public partial class ColorField : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(Color), typeof(ColorField),
            new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public Color Value
    {
        get => (Color)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private double _hue;
    private double _saturation;
    private double _brightness;
    private double _alpha;
    private Color _committedColor;
    private string _hexValue = "";

    public ColorField()
    {
        InitializeComponent();

        HueSlider.ValueChanged += (_, _) =>
        {
            _hue = HueSlider.Value;
            
            CommitHsv();
        };

        SvPicker.ValueChanged += (_, _) =>
        {
            _saturation = SvPicker.Saturation;
            _brightness = SvPicker.Brightness;
            
            CommitHsv();
        };

        AlphaSlider.ValueChanged += (_, _) =>
        {
            _alpha = AlphaSlider.Value;
            
            CommitHsv();
        };

        Loaded += (_, _) =>
        {
            UpdateFromValue(Value);
        };
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ColorField field)
        {
            return;
        }
        
        var newColor = (Color)e.NewValue;

        if (newColor == field._committedColor)
        {
            return;
        }
        
        field.UpdateFromValue(newColor);
    }

    private void UpdateFromValue(Color color)
    {
        _committedColor = color;
        
        ColorUtil.RgbToHsv(color, out _hue, out _saturation, out _brightness);
        
        UpdateSvPicker();
        UpdateHueSlider();
        UpdateAlphaSlider(color);
        UpdateHexTextBox(color);
        UpdatePreview(color);
    }

    private void CommitHsv()
    {
        var color = ColorUtil.HsvToRgb(_hue, _saturation, _brightness, _alpha);
        
        _committedColor = color;
        
        Value = color;
        
        UpdateSvPicker();
        UpdateHueSlider();
        UpdateAlphaSlider(color);
        UpdateHexTextBox(color);
        UpdatePreview(color);
    }

    private void HexTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (HexTextBox.Text == _hexValue)
        {
            return;
        }
        
        _hexValue = HexTextBox.Text;

        if (!ColorUtil.TryParseHex(_hexValue, out var color))
        {
            return;
        }
        
        ColorUtil.RgbToHsv(color, out _hue, out _saturation, out _brightness);
        _committedColor = color;
        Value = color;
        
        UpdateSvPicker();
        UpdateHueSlider();
        UpdateAlphaSlider(color);
        UpdatePreview(color);
    }

    private void UpdateSvPicker()
    {
        SvPicker.HueColor = ColorUtil.HsvToRgb(_hue, 1, 1, 1);
        SvPicker.Saturation = _saturation;
        SvPicker.Brightness = _brightness;
    }

    private void UpdateHueSlider()
    {
        HueSlider.Value = _hue;
    }

    private void UpdateAlphaSlider(Color color)
    {
        AlphaSlider.BaseColor = Color.FromArgb(255, color.R, color.G, color.B);
        AlphaSlider.Value = color.A;
    }

    private void UpdateHexTextBox(Color color)
    {
        _hexValue = ColorUtil.ToHexString(color);
        HexTextBox.Text = _hexValue;
    }

    private void UpdatePreview(Color color)
    {
        PreviewRect.Fill = new SolidColorBrush(color);
    }
}
