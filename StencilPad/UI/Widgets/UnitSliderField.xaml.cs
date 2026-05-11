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
        DependencyProperty.Register(nameof(Minimum), typeof(Unit), typeof(UnitSliderField),
            new FrameworkPropertyMetadata(Unit.FromMillimeters(0), OnConstraintChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(Unit), typeof(UnitSliderField),
            new FrameworkPropertyMetadata(Unit.FromMillimeters(1000000), OnConstraintChanged));

    ////////////////////////////////////////
    
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
        UpdateTextValue();
        UpdateSliderValue();
    }

    private bool _isUpdating;

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UnitSliderField field || field._isUpdating)
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
                field.UpdateSliderValue();
            }

            if (e.Property == TextValueProperty || e.Property == UnitTypeProperty)
            {
                if (Unit.TryParse(field.TextValue, field.UnitType, out var parsedValue))
                {
                    var clamped = field.ClampValue(parsedValue);
                    
                    field.Value = clamped;
                    field.UpdateSliderValue();
                }
            }
        }
        finally
        {
            field._isUpdating = false;
        }
    }

    private static void OnSliderValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UnitSliderField field || field._isUpdating)
        {
            return;
        }
        
        field._isUpdating = true;
        
        try
        {
            var unit = Unit.FromType((decimal)field.SliderValue, field.UnitType);
            var clamped = field.ClampValue(unit);

            field.Value = clamped;
            field.UpdateTextValue();
        }
        finally
        {
            field._isUpdating = false;
        }
    }

    private static void OnConstraintChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UnitSliderField field || field._isUpdating)
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
            field.UpdateSliderValue();
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

    private void UpdateSliderValue()
    {
        SliderValue = Value.ToType(UnitType);
    }
    
    private Unit ClampValue(Unit value)
    {
        return Unit.Clamp(value, Minimum, Maximum);
    }
}
