using System.Windows;
using System.Windows.Controls;

namespace StencilPad.UI;

public partial class GridButtons : UserControl
{
    public static readonly DependencyProperty ShowGridProperty =
        DependencyProperty.Register(nameof(ShowGrid), typeof(bool), typeof(GridButtons),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SnapToGridProperty =
        DependencyProperty.Register(nameof(SnapToGrid), typeof(bool), typeof(GridButtons),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SnapToPointProperty =
        DependencyProperty.Register(nameof(SnapToPoint), typeof(bool), typeof(GridButtons),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public bool SnapToGrid
    {
        get => (bool)GetValue(SnapToGridProperty);
        set => SetValue(SnapToGridProperty, value);
    }

    public bool SnapToPoint
    {
        get => (bool)GetValue(SnapToPointProperty);
        set => SetValue(SnapToPointProperty, value);
    }
    
    public GridButtons()
    {
        InitializeComponent();
    }
}
