using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StencilPad.UI.Widgets;

public partial class ColorFieldAlphaSlider : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(ColorFieldAlphaSlider),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty BaseColorProperty =
        DependencyProperty.Register(nameof(BaseColor), typeof(Color), typeof(ColorFieldAlphaSlider),
            new FrameworkPropertyMetadata(Colors.Black, OnBaseColorChanged));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Color BaseColor
    {
        get => (Color)GetValue(BaseColorProperty);
        set => SetValue(BaseColorProperty, value);
    }

    private bool _dragging;

    public event EventHandler? ValueChanged;

    public ColorFieldAlphaSlider()
    {
        InitializeComponent();

        GradientRect.Fill = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0.5),
            EndPoint = new Point(1, 0.5),
            GradientStops =
            {
                new GradientStop(Colors.Transparent, 0),
                new GradientStop(Colors.Black, 1)
            }
        };

        Loaded += (_, _) => UpdateMarkerPosition();
        SizeChanged += (_, _) => UpdateMarkerPosition();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ColorFieldAlphaSlider slider)
        {
            return;
        }
        
        slider.UpdateMarkerPosition();
    }

    private static void OnBaseColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ColorFieldAlphaSlider slider)
        {
            return;
        }
        
        slider.UpdateGradient();
    }

    private void DragCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _dragging = true;
        DragCanvas.CaptureMouse();
        SetValueFromPoint(e.GetPosition(DragCanvas));
    }

    private void DragCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_dragging)
        {
            SetValueFromPoint(e.GetPosition(DragCanvas));
        }
    }

    private void DragCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_dragging)
        {
            return;
        }
        
        _dragging = false;
        DragCanvas.ReleaseMouseCapture();
    }

    private void SetValueFromPoint(Point p)
    {
        Value = Math.Clamp(p.X / DragCanvas.ActualWidth, 0, 1);
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateGradient()
    {
        var brush = (LinearGradientBrush)GradientRect.Fill;
        var c = BaseColor;

        brush.GradientStops[0].Color = Color.FromArgb(0, c.R, c.G, c.B);
        brush.GradientStops[1].Color = Color.FromArgb(255, c.R, c.G, c.B);
    }

    private void UpdateMarkerPosition()
    {
        if (!IsLoaded)
        {
            return;
        }
        
        var x = Value * DragCanvas.ActualWidth;

        Canvas.SetLeft(Marker, x - Marker.Width / 2);
        Canvas.SetTop(Marker, (DragCanvas.ActualHeight - Marker.Height) / 2);
    }
}
