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
    private bool _isUpdating;
    private bool _draggingSv;
    private bool _draggingHue;
    private bool _draggingAlpha;

    public ColorField()
    {
        InitializeComponent();

        AlphaBarRect.Fill = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0.5),
            EndPoint = new Point(1, 0.5),
            GradientStops =
            {
                new GradientStop(Colors.Transparent, 0),
                new GradientStop(Colors.Black, 1)
            }
        };

        Loaded += (_, _) => UpdateFromValue(Value);
        SizeChanged += (_, _) => UpdateMarkerPositions();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ColorField field || field._isUpdating)
        {
            return;
        }
        
        field.UpdateFromValue((Color)e.NewValue);
    }

    private void UpdateFromValue(Color color)
    {
        _isUpdating = true;

        try
        {
            ColorUtil.RgbToHsv(color, out _hue, out _saturation, out _brightness);
            UpdateSvGradient();
            UpdateAlphaGradient();
            UpdateMarkerPositions();
            UpdateHexTextBox();
            UpdatePreview();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void CommitHsv(byte alpha)
    {
        var color = ColorUtil.HsvToRgb(_hue, _saturation, _brightness, alpha);

        _isUpdating = true;

        try
        {
            Value = color;
            UpdateSvGradient();
            UpdateAlphaGradient();
            UpdateMarkerPositions();
            UpdateHexTextBox();
            UpdatePreview();
        }
        finally
        {
            _isUpdating = false;
        }
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
        {
            SetSvFromPoint(e.GetPosition(SvCanvas));
        }
    }

    private void SvCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_draggingSv)
        {
            return;
        }
        
        _draggingSv = false;
        SvCanvas.ReleaseMouseCapture();
    }

    private void SetSvFromPoint(Point p)
    {
        _saturation = Math.Clamp(p.X / SvCanvas.ActualWidth, 0, 1);
        _brightness = Math.Clamp(1 - p.Y / SvCanvas.ActualHeight, 0, 1);
        CommitHsv(Value.A);
    }

    private void HueCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _draggingHue = true;
        HueCanvas.CaptureMouse();
        SetHueFromPoint(e.GetPosition(HueCanvas));
    }

    private void HueCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_draggingHue)
        {
            SetHueFromPoint(e.GetPosition(HueCanvas));
        }
    }

    private void HueCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_draggingHue)
        {
            return;
        }
        
        _draggingHue = false;
        HueCanvas.ReleaseMouseCapture();
    }

    private void SetHueFromPoint(Point p)
    {
        _hue = Math.Clamp(p.X / HueCanvas.ActualWidth, 0, 1) * 360;
        CommitHsv(Value.A);
    }

    private void AlphaCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _draggingAlpha = true;
        
        AlphaCanvas.CaptureMouse();
        
        SetAlphaFromPoint(e.GetPosition(AlphaCanvas));
    }

    private void AlphaCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_draggingAlpha)
        {
            SetAlphaFromPoint(e.GetPosition(AlphaCanvas));
        }
    }

    private void AlphaCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_draggingAlpha)
        {
            return;
        }
        
        _draggingAlpha = false;
        AlphaCanvas.ReleaseMouseCapture();
    }

    private void SetAlphaFromPoint(Point p)
    {
        var alpha = (byte)Math.Clamp(p.X / AlphaCanvas.ActualWidth * 255, 0, 255);
        
        CommitHsv(alpha);
    }

    private void HexTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating)
        {
            return;
        }
        
        if (!ColorUtil.TryParseHex(HexTextBox.Text, out var color))
        {
            return;
        }
        
        _isUpdating = true;

        try
        {
            ColorUtil.RgbToHsv(color, out _hue, out _saturation, out _brightness);
            
            Value = color;
            
            UpdateSvGradient();
            UpdateAlphaGradient();
            UpdateMarkerPositions();
            UpdatePreview();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void UpdateSvGradient()
    {
        var brush = (LinearGradientBrush)SvSaturationRect.Fill;
        
        brush.GradientStops[1].Color = ColorUtil.HsvToRgb(_hue, 1, 1, 255);
    }

    private void UpdateAlphaGradient()
    {
        var brush = (LinearGradientBrush)AlphaBarRect.Fill;
        
        brush.GradientStops[0].Color = Color.FromArgb(0, Value.R, Value.G, Value.B);
        brush.GradientStops[1].Color = Color.FromArgb(255, Value.R, Value.G, Value.B);
    }

    private void UpdateMarkerPositions()
    {
        if (!IsLoaded)
            return;

        var svX = _saturation * SvCanvas.ActualWidth;
        var svY = (1 - _brightness) * SvCanvas.ActualHeight;

        Canvas.SetLeft(SvMarkerOuter, svX - SvMarkerOuter.Width / 2);
        Canvas.SetTop(SvMarkerOuter, svY - SvMarkerOuter.Height / 2);
        Canvas.SetLeft(SvMarker, svX - SvMarker.Width / 2);
        Canvas.SetTop(SvMarker, svY - SvMarker.Height / 2);

        var hueX = _hue / 360.0 * HueCanvas.ActualWidth;

        Canvas.SetLeft(HueMarker, hueX - HueMarker.Width / 2);
        Canvas.SetTop(HueMarker, (HueCanvas.ActualHeight - HueMarker.Height) / 2);

        var alphaX = Value.A / 255.0 * AlphaCanvas.ActualWidth;

        Canvas.SetLeft(AlphaMarker, alphaX - AlphaMarker.Width / 2);
        Canvas.SetTop(AlphaMarker, (AlphaCanvas.ActualHeight - AlphaMarker.Height) / 2);
    }

    private void UpdateHexTextBox()
    {
        HexTextBox.Text = ColorUtil.ToHexString(Value);
    }

    private void UpdatePreview()
    {
        PreviewRect.Fill = new SolidColorBrush(Value);
    }

    // Color conversions

}
