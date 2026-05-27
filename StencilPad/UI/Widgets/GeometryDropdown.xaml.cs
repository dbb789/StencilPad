using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StencilPad.UI.Widgets;

public partial class GeometryDropdown : UserControl
{
    public sealed record Entry(Geometry Geometry, DashStyle? DashStyle = null);

    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(IList<Entry>), typeof(GeometryDropdown),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(GeometryDropdown),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public IList<Entry> Items
    {
        get => (IList<Entry>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public GeometryDropdown()
    {
        InitializeComponent();
    }
}
