using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StencilPad.Spatial;

namespace StencilPad.UI.Widgets;

public enum CornerSizeField_Mode
{
    Millimeters,
    Inches,
    Proportion
}

public class CornerSizeField_Item
{
    public CornerSizeField_Mode Value { get; set; }
    public string Description { get; set; } = "";
}

public partial class CornerSizeField : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(CornerSize), typeof(CornerSizeField),
            new FrameworkPropertyMetadata(CornerSize.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty SizeModeProperty =
        DependencyProperty.Register(nameof(SizeMode), typeof(CornerSizeField_Mode), typeof(CornerSizeField),
            new FrameworkPropertyMetadata(CornerSizeField_Mode.Millimeters, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty TextValueProperty =
        DependencyProperty.Register(nameof(TextValue), typeof(string), typeof(CornerSizeField),
            new FrameworkPropertyMetadata("0", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public CornerSize Value
    {
        get => (CornerSize)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public CornerSizeField_Mode SizeMode
    {
        get => (CornerSizeField_Mode)GetValue(SizeModeProperty);
        set => SetValue(SizeModeProperty, value);
    }

    public string TextValue
    {
        get => (string)GetValue(TextValueProperty);
        set => SetValue(TextValueProperty, value);
    }

    public CornerSizeField()
    {
        InitializeComponent();
    }

    private void ValueField_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            ValueField.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
    }
    
    private bool _isUpdating;

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CornerSizeField field || field._isUpdating)
        {
            return;
        }
        
        field._isUpdating = true;
        
        try
        {
            if (e.Property == ValueProperty || e.Property == SizeModeProperty)
            {
                var size = field.Value;
                
                if (field.SizeMode == CornerSizeField_Mode.Proportion)
                {
                    if (size.IsProportion)
                    {
                        field.TextValue = (size.Proportion * 100).ToString("0.###", CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    var unitType = field.SizeMode == CornerSizeField_Mode.Inches ? UnitType.Inches : UnitType.Millimeters;

                    if (size.IsUnit)
                    {
                        field.TextValue = size.Unit.ToType(unitType).ToString("0.###", CultureInfo.InvariantCulture);
                    }
                }
            }

            if (e.Property == TextValueProperty || e.Property == SizeModeProperty)
            {
                if (field.SizeMode == CornerSizeField_Mode.Proportion)
                {
                    if (double.TryParse(field.TextValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var pct))
                    {
                        field.Value = CornerSize.FromProportion(pct / 100.0);
                    }
                }
                else
                {
                    var unitType = field.SizeMode == CornerSizeField_Mode.Inches ? UnitType.Inches : UnitType.Millimeters;

                    if (Unit.TryParse(field.TextValue, unitType, out var parsedUnit))
                    {
                        field.Value = CornerSize.FromUnit(parsedUnit);
                    }
                }
            }
        }
        finally
        {
            field._isUpdating = false;
        }
    }
}
