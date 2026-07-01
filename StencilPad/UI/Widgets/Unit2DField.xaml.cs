using System.Windows;
using System.Windows.Controls;
using StencilPad.Spatial;

namespace StencilPad.UI.Widgets;

public partial class Unit2DField : UserControl
{
    public static readonly DependencyProperty ValueXProperty =
        DependencyProperty.Register(nameof(ValueX), typeof(Unit?), typeof(Unit2DField),
            new FrameworkPropertyMetadata(Unit.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    
    public static readonly DependencyProperty ValueYProperty =
        DependencyProperty.Register(nameof(ValueY), typeof(Unit?), typeof(Unit2DField),
            new FrameworkPropertyMetadata(Unit.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty UnitTypeProperty =
        DependencyProperty.Register(nameof(UnitType), typeof(UnitType), typeof(Unit2DField),
            new FrameworkPropertyMetadata(UnitType.Millimeters, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty UnitSettingsProperty =
        DependencyProperty.Register(nameof(UnitSettings), typeof(UnitSettings), typeof(Unit2DField),
            new FrameworkPropertyMetadata(UnitSettings.Default, FrameworkPropertyMetadataOptions.None));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(Unit), typeof(Unit2DField),
            new FrameworkPropertyMetadata(Unit.FromMillimeters(-1000000)));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(Unit), typeof(Unit2DField),
            new FrameworkPropertyMetadata(Unit.FromMillimeters(1000000)));

    public static readonly DependencyProperty ScaledProperty =
        DependencyProperty.Register(nameof(Scaled), typeof(bool), typeof(Unit2DField),
            new FrameworkPropertyMetadata(false));
    
    public Unit? ValueX
    {
        get => (Unit?)GetValue(ValueXProperty);
        set => SetValue(ValueXProperty, value);
    }

    public Unit? ValueY
    {
        get => (Unit?)GetValue(ValueYProperty);
        set => SetValue(ValueYProperty, value);
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

    public Unit2DField()
    {
        InitializeComponent();
    }
}
