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
        DependencyProperty.Register(nameof(Minimum), typeof(Unit), typeof(UnitField),
            new FrameworkPropertyMetadata(Unit.FromMillimeters(0), OnConstraintChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(Unit), typeof(UnitField),
            new FrameworkPropertyMetadata(Unit.FromMillimeters(1000000), OnConstraintChanged));
    
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

    public Unit Minimum
    {
        get => (Unit)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public Unit Maximum
    {
        get => (Unit)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public UnitField()
    {
        InitializeComponent();
        UpdateTextValue();
    }

    private bool _isUpdating;

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UnitField field || field._isUpdating)
        {
            return;
        }

        field._isUpdating = true;
        
        try
        {
            if (e.Property == ValueProperty || e.Property == UnitTypeProperty)
            {
                var clamped = field.ClampValue(field.Value);

                if (clamped != field.Value)
                {
                    field.Value = clamped;
                }

                field.UpdateTextValue();
            }

            if (e.Property == TextValueProperty || e.Property == UnitTypeProperty)
            {
                if (Unit.TryParse(field.TextValue, field.UnitType, out var parsedValue))
                {
                    field.Value = field.ClampValue(parsedValue);
                }
            }
        }
        finally
        {
            field._isUpdating = false;
        }
    }

    private static void OnConstraintChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UnitField field || field._isUpdating)
        {
            return;
        }
        
        field._isUpdating = true;
        
        try
        {
            var clamped = field.ClampValue(field.Value);

            if (clamped != field.Value)
            {
                field.Value = clamped;
            }

            field.UpdateTextValue();
        }
        finally
        {
            field._isUpdating = false;
        }
    }

    private void UpdateTextValue()
    {
        TextValue = Value.ToType(UnitType).ToString("0.###");
    }
    
    private Unit ClampValue(Unit value)
    {
        return Unit.Clamp(value, Minimum, Maximum);
    }
}

public class UnitItem
{
    public UnitType Value { get; set; }
    public string Description { get; set; } = "";
}
