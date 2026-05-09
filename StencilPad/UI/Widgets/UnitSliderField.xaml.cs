using System.Windows;
using System.Windows.Controls;
using StencilPad.Spatial;

namespace StencilPad.UI.Widgets;

public partial class UnitSliderField : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(Unit), typeof(UnitSliderField),
            new FrameworkPropertyMetadata(Unit.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty UnitTypeProperty =
        DependencyProperty.Register(nameof(UnitType), typeof(UnitType), typeof(UnitSliderField),
            new FrameworkPropertyMetadata(UnitType.Millimeters, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty TextValueProperty =
        DependencyProperty.Register(nameof(TextValue), typeof(string), typeof(UnitSliderField),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(Unit?), typeof(UnitSliderField),
            new FrameworkPropertyMetadata(null, OnConstraintChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(Unit?), typeof(UnitSliderField),
            new FrameworkPropertyMetadata(null, OnConstraintChanged));

    // Internal slider bindings (in current display unit)
    public static readonly DependencyProperty SliderValueProperty =
        DependencyProperty.Register(nameof(SliderValue), typeof(double), typeof(UnitSliderField),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSliderValueChanged));

    public static readonly DependencyProperty SliderMinimumProperty =
        DependencyProperty.Register(nameof(SliderMinimum), typeof(double), typeof(UnitSliderField),
            new FrameworkPropertyMetadata(0.0));

    public static readonly DependencyProperty SliderMaximumProperty =
        DependencyProperty.Register(nameof(SliderMaximum), typeof(double), typeof(UnitSliderField),
            new FrameworkPropertyMetadata(100.0));

    public Unit Value
    {
        get => (Unit)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public UnitType UnitType
    {
        get => (UnitType)GetValue(UnitTypeProperty);
        set => SetValue(UnitTypeProperty, value);
    }

    public string TextValue
    {
        get => (string)GetValue(TextValueProperty);
        set => SetValue(TextValueProperty, value);
    }

    public Unit? Minimum
    {
        get => (Unit?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public Unit? Maximum
    {
        get => (Unit?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double SliderValue
    {
        get => (double)GetValue(SliderValueProperty);
        set => SetValue(SliderValueProperty, value);
    }

    public double SliderMinimum
    {
        get => (double)GetValue(SliderMinimumProperty);
        set => SetValue(SliderMinimumProperty, value);
    }

    public double SliderMaximum
    {
        get => (double)GetValue(SliderMaximumProperty);
        set => SetValue(SliderMaximumProperty, value);
    }

    public UnitSliderField()
    {
        InitializeComponent();
    }

    private bool _isUpdating;

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UnitSliderField f || f._isUpdating)
            return;

        f._isUpdating = true;
        try
        {
            if (e.Property == ValueProperty || e.Property == UnitTypeProperty)
            {
                var clamped = f.ClampValue(f.Value);
                if (clamped != f.Value)
                    f.Value = clamped;
                f.UpdateSliderRange();
                f.TextValue = clamped.ToType(f.UnitType).ToString("0.###");
                f.SliderValue = clamped.ToType(f.UnitType);
            }

            if (e.Property == TextValueProperty || e.Property == UnitTypeProperty)
            {
                if (Unit.TryParse(f.TextValue, f.UnitType, out var parsedValue))
                {
                    var clamped = f.ClampValue(parsedValue);
                    f.Value = clamped;
                    f.SliderValue = clamped.ToType(f.UnitType);
                }
            }
        }
        finally
        {
            f._isUpdating = false;
        }
    }

    private static void OnSliderValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UnitSliderField f || f._isUpdating)
            return;

        f._isUpdating = true;
        try
        {
            var unit = Unit.FromType((decimal)f.SliderValue, f.UnitType);
            var clamped = f.ClampValue(unit);
            f.Value = clamped;
            f.TextValue = clamped.ToType(f.UnitType).ToString("0.###");
        }
        finally
        {
            f._isUpdating = false;
        }
    }

    private static void OnConstraintChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UnitSliderField f || f._isUpdating)
            return;

        f._isUpdating = true;
        try
        {
            f.UpdateSliderRange();
            var clamped = f.ClampValue(f.Value);
            if (clamped != f.Value)
                f.Value = clamped;
            f.TextValue = clamped.ToType(f.UnitType).ToString("0.###");
            f.SliderValue = clamped.ToType(f.UnitType);
        }
        finally
        {
            f._isUpdating = false;
        }
    }

    private void UpdateSliderRange()
    {
        SliderMinimum = Minimum?.ToType(UnitType) ?? 0.0;
        SliderMaximum = Maximum?.ToType(UnitType) ?? 100.0;
    }

    private Unit ClampValue(Unit value)
    {
        if (Minimum.HasValue && Maximum.HasValue)
            return Unit.Clamp(value, Minimum.Value, Maximum.Value);
        if (Minimum.HasValue && value < Minimum.Value)
            return Minimum.Value;
        if (Maximum.HasValue && value > Maximum.Value)
            return Maximum.Value;
        return value;
    }
}
