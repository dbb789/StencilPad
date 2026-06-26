using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StencilPad.UI.Widgets;

public partial class AlphaField : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(AlphaField),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(AlphaField),
            new FrameworkPropertyMetadata("A"));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public event Action? DragBegin;
    public event Action? DragEnd;

    public AlphaField()
    {
        InitializeComponent();

        AlphaSlider.DragBegin += () => DragBegin?.Invoke();
        AlphaSlider.DragEnd += () => DragEnd?.Invoke();
    }
}
