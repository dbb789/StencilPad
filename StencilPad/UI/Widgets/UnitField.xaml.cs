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

    private string _textValue = "";

    public UnitField()
    {
        InitializeComponent();
        UpdateTextValue();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UnitField field)
        {
            return;
        }

        if (e.Property == ValueProperty || e.Property == UnitTypeProperty)
        {
            var clamped = field.ClampValue(field.Value);
            
            if (clamped != field.Value)
            {
                field.Value = clamped;
            }
            
            field.UpdateTextValue();
        }
    }

    private static void OnConstraintChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UnitField field)
        {
            return;
        }
        
        var clamped = field.ClampValue(field.Value);
        
        if (clamped != field.Value)
        {
            field.Value = clamped;
        }

        field.UpdateTextValue();
    }

    private void TextBoxChanged(object sender, TextChangedEventArgs e)
    {
        if (_textValue != TextBox.Text)
        {
            _textValue = TextBox.Text;

            if (Unit.TryParse(_textValue, UnitType, out var parsed))
            {
                Value = ClampValue(parsed);
            }
        }
    }

    private void UpdateTextValue()
    {
        _textValue = Value.ToType(UnitType).ToString("0.###");
        TextBox.Text = _textValue;
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
