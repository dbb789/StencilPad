using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StencilPad.Spatial;

namespace StencilPad.UI.Widgets;

public partial class BaseUnitField : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(Unit?), typeof(BaseUnitField),
            new FrameworkPropertyMetadata(Unit.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));
    
    public static readonly DependencyProperty UnitTypeProperty =
        DependencyProperty.Register(nameof(UnitType), typeof(UnitType), typeof(BaseUnitField),
            new FrameworkPropertyMetadata(UnitType.Millimeters, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty UnitSettingsProperty =
        DependencyProperty.Register(nameof(UnitSettings), typeof(UnitSettings), typeof(BaseUnitField),
            new FrameworkPropertyMetadata(UnitSettings.Default, FrameworkPropertyMetadataOptions.None, OnUnitSettingsChanged));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(Unit), typeof(BaseUnitField),
            new FrameworkPropertyMetadata(OnValueChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(Unit), typeof(BaseUnitField),
            new FrameworkPropertyMetadata(OnValueChanged));

    public static readonly DependencyProperty ScaledProperty =
        DependencyProperty.Register(nameof(Scaled), typeof(bool), typeof(BaseUnitField),
            new FrameworkPropertyMetadata(false, OnValueChanged));
    
    public Unit? Value
    {
        get => (Unit?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    public UnitType UnitType
    {
        get => (UnitType)GetValue(UnitTypeProperty);
        set => SetValue(UnitTypeProperty, value);
    }

    public UnitSettings UnitSettings
    {
        get => (UnitSettings)GetValue(UnitSettingsProperty);
        set => SetValue(UnitSettingsProperty, value);
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

    public bool Scaled
    {
        get => (bool)GetValue(ScaledProperty);
        set => SetValue(ScaledProperty, value);
    }

    private string _textValue = "";

    public BaseUnitField()
    {
        InitializeComponent();
        UpdateTextValue();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not BaseUnitField field)
        {
            return;
        }

        if (field.Value is not null)
        {
            var clamped = field.ClampValue(field.Value.Value);

            if (clamped != field.Value)
            {
                field.Value = clamped;
            }
        }

        field.UpdateTextValue();
    }

    private static void OnUnitSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not BaseUnitField field)
        {
            return;
        }

        field.UnitType = UnitUtil.GetDefaultUnitType(field.UnitSettings);
        field.UpdateTextValue();
    }
    
    private void ValueField_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            ApplyValueField();
        }
    }

    private void Up_Click(object sender, RoutedEventArgs e)
    {
        ApplyValueField();

        if (Value is not null)
        {
            Value = ClampValue(Value.Value + GetStep());
        }
    }

    private void Down_Click(object sender, RoutedEventArgs e)
    {
        ApplyValueField();

        if (Value is not null)
        {
            Value = ClampValue(Value.Value - GetStep());
        }
    }

    private Unit GetStep()
    {
        var majorStep = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

        if (UnitType == UnitType.Millimeters)
        {
            return Unit.FromMillimeters(majorStep ? 1 : 0.1);
        }
        else
        {
            return Unit.FromInches(majorStep ? 0.25 : 0.0625);
        }
    }

    private void ApplyValueField()
    {
        if (_textValue == ValueField.Text)
        {
            return;
        }

        _textValue = ValueField.Text;

        if (Scaled)
        {
            if (Unit.TryParse(_textValue, UnitType, UnitSettings.Ratio, out var parsed))
            {
                Value = ClampValue(parsed);
            }
        }
        else
        {
            if (Unit.TryParse(_textValue, UnitType, out var parsed))
            {
                Value = ClampValue(parsed);
            }
        }
    }

    private void UpdateTextValue()
    {
        if (Value is null)
        {
            _textValue = "";
            ValueField.Text = "";
            return;
        }

        if (Scaled)
        {
            _textValue = UnitUtil.FormatScaled(Value.Value, UnitType, UnitSettings);
        }
        else
        {
            _textValue = UnitUtil.Format(Value.Value, UnitType);
        }
        
        ValueField.Text = _textValue;
    }
    
    private Unit ClampValue(Unit value)
    {
        return Unit.Clamp(value, Minimum, Maximum);
    }
}
