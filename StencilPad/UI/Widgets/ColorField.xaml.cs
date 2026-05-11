using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
    private Color _committedColor;
    private string _hexValue = "";
    private bool _draggingSv;

    public ColorField()
    {
        InitializeComponent();

        HueSlider.ValueChanged += (_, _) =>
        {
            _hue = HueSlider.Value;
            CommitHsv(Value.A);
        };

        AlphaSlider.ValueChanged += (_, _) => CommitHsv(AlphaSlider.Value);

        Loaded += (_, _) => UpdateFromValue(Value);
        SizeChanged += (_, _) => UpdateSvMarkerPosition();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ColorField field)
            return;

        var newColor = (Color)e.NewValue;

        if (newColor == field._committedColor)
            return;

        field.UpdateFromValue(newColor);
    }

    private void UpdateFromValue(Color color)
    {
        _committedColor = color;
        ColorUtil.RgbToHsv(color, out _hue, out _saturation, out _brightness);
        UpdateSvGradient();
        UpdateHueSlider();
        UpdateAlphaSlider(color);
        UpdateSvMarkerPosition();
        UpdateHexTextBox(color);
        UpdatePreview(color);
    }

    private void CommitHsv(byte alpha)
    {
        var color = ColorUtil.HsvToRgb(_hue, _saturation, _brightness, alpha);
        _committedColor = color;
        Value = color;
        UpdateSvGradient();
        UpdateHueSlider();
        UpdateAlphaSlider(color);
        UpdateSvMarkerPosition();
        UpdateHexTextBox(color);
        UpdatePreview(color);
    }

    // SV canvas

    private void SvCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _draggingSv = true;
        SvCanvas.CaptureMouse();
        SetSvFromPoint(e.GetPosition(SvCanvas));
    }

    private void SvCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_draggingSv)
            SetSvFromPoint(e.GetPosition(SvCanvas));
    }

    private void SvCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_draggingSv)
            return;

        _draggingSv = false;
        SvCanvas.ReleaseMouseCapture();
    }

    private void SetSvFromPoint(Point p)
    {
        _saturation = Math.Clamp(p.X / SvCanvas.ActualWidth, 0, 1);
        _brightness = Math.Clamp(1 - p.Y / SvCanvas.ActualHeight, 0, 1);
        CommitHsv(Value.A);
    }

    // Hex text box

    private void HexTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (HexTextBox.Text == _hexValue)
            return;

        _hexValue = HexTextBox.Text;

        if (!ColorUtil.TryParseHex(_hexValue, out var color))
            return;

        ColorUtil.RgbToHsv(color, out _hue, out _saturation, out _brightness);
        _committedColor = color;
        Value = color;
        UpdateSvGradient();
        UpdateHueSlider();
        UpdateAlphaSlider(color);
        UpdateSvMarkerPosition();
        UpdatePreview(color);
    }

    // Update helpers

    private void UpdateSvGradient()
    {
        var brush = (LinearGradientBrush)SvSaturationRect.Fill;
        brush.GradientStops[1].Color = ColorUtil.HsvToRgb(_hue, 1, 1, 255);
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

    private void UpdateSvMarkerPosition()
    {
        if (!IsLoaded)
            return;

        var svX = _saturation * SvCanvas.ActualWidth;
        var svY = (1 - _brightness) * SvCanvas.ActualHeight;

        Canvas.SetLeft(SvMarkerOuter, svX - SvMarkerOuter.Width / 2);
        Canvas.SetTop(SvMarkerOuter, svY - SvMarkerOuter.Height / 2);
        Canvas.SetLeft(SvMarker, svX - SvMarker.Width / 2);
        Canvas.SetTop(SvMarker, svY - SvMarker.Height / 2);
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
