using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

    public static readonly DependencyProperty UnitSettingsProperty =
        DependencyProperty.Register(nameof(UnitSettings), typeof(UnitSettings), typeof(UnitSliderField),
            new FrameworkPropertyMetadata(UnitSettings.Default, FrameworkPropertyMetadataOptions.None, OnUnitSettingsChanged));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(Unit), typeof(UnitSliderField),
            new FrameworkPropertyMetadata(Unit.FromMillimeters(0), OnConstraintChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(Unit), typeof(UnitSliderField),
            new FrameworkPropertyMetadata(Unit.FromMillimeters(1000000), OnConstraintChanged));

    public static readonly DependencyProperty SliderMinimumProperty =
        DependencyProperty.Register(nameof(SliderMinimum), typeof(double), typeof(UnitSliderField), new FrameworkPropertyMetadata(0.0));

    public static readonly DependencyProperty SliderMaximumProperty =
        DependencyProperty.Register(nameof(SliderMaximum), typeof(double), typeof(UnitSliderField), new FrameworkPropertyMetadata(100.0));

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

    private string _textValue = "";
    private double _sliderValue = 0.0;
    
    public UnitSliderField()
    {
        InitializeComponent();
        UpdateTextValue();
        UpdateSliderValue();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UnitSliderField field)
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
            field.UpdateSliderValue();
        }
    }

    private static void OnConstraintChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UnitSliderField field)
        {
            return;
        }

        var clamped = field.ClampValue(field.Value);
        
        if (clamped != field.Value)
        {
            field.Value = clamped;
        }
        
        field.UpdateTextValue();
        field.UpdateSliderValue();
    }
    
    private static void OnUnitSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UnitSliderField field)
        {
            return;
        }

        field.UnitType = UnitUtil.GetDefaultUnitType(field.UnitSettings);
    }

    private void ValueField_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            ApplyValueField();
        }
    }

    private void ApplyValueField()
    {
        if (_textValue == ValueField.Text)
        {
            return;
        }
        
        _textValue = ValueField.Text;

        if (Unit.TryParse(_textValue, UnitType, out var parsed))
        {
            Value = ClampValue(parsed);
        }
    }

    private void SliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (Math.Abs(_sliderValue - Slider.Value) > 0.00001)
        {
            _sliderValue = Slider.Value;
            
            Value = ClampValue(Unit.FromType((decimal)_sliderValue, UnitType));
        }
    }

    private void UpdateTextValue()
    {
        _textValue = Value.ToType(UnitType).ToString("0.###");
        ValueField.Text = _textValue;
    }

    private void UpdateSliderValue()
    {
        _sliderValue = Math.Clamp(Value.ToType(UnitType), SliderMinimum, SliderMaximum);
        Slider.Value = _sliderValue;
    }
    
    private Unit ClampValue(Unit value)
    {
        return Unit.Clamp(value, Minimum, Maximum);
    }
}
