using System.Windows;
using System.Windows.Controls;
using StencilPad.Spatial;

namespace StencilPad.UI.Widgets;

public partial class UnitField : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(Unit), typeof(UnitField),
            new FrameworkPropertyMetadata(Unit.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));
    
    public static readonly DependencyProperty UnitTypeProperty =
        DependencyProperty.Register(nameof(UnitType), typeof(UnitType), typeof(UnitField),
            new FrameworkPropertyMetadata(UnitType.Millimeters, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty TextValueProperty =
        DependencyProperty.Register(nameof(TextValue), typeof(string), typeof(UnitField),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(Unit?), typeof(UnitField),
            new FrameworkPropertyMetadata(null, OnConstraintChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(Unit?), typeof(UnitField),
            new FrameworkPropertyMetadata(null, OnConstraintChanged));
    
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

    public UnitField()
    {
        InitializeComponent();
    }

    private bool _isUpdating;

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UnitField unitField || unitField._isUpdating)
        {
            return;
        }

        unitField._isUpdating = true;
        try
        {
            if (e.Property == ValueProperty || e.Property == UnitTypeProperty)
            {
                var clamped = unitField.ClampValue(unitField.Value);
                if (clamped != unitField.Value)
                    unitField.Value = clamped;
                unitField.TextValue = clamped.ToType(unitField.UnitType).ToString("0.###");
            }

            if (e.Property == TextValueProperty || e.Property == UnitTypeProperty)
            {
                if (Unit.TryParse(unitField.TextValue, unitField.UnitType, out var parsedValue))
                {
                    unitField.Value = unitField.ClampValue(parsedValue);
                }
            }
        }
        finally
        {
            unitField._isUpdating = false;
        }
    }

    private static void OnConstraintChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UnitField unitField || unitField._isUpdating)
            return;

        unitField._isUpdating = true;
        try
        {
            var clamped = unitField.ClampValue(unitField.Value);
            if (clamped != unitField.Value)
                unitField.Value = clamped;
            unitField.TextValue = clamped.ToType(unitField.UnitType).ToString("0.###");
        }
        finally
        {
            unitField._isUpdating = false;
        }
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

public class UnitItem
{
    public UnitType Value { get; set; }
    public string Description { get; set; } = "";
}
