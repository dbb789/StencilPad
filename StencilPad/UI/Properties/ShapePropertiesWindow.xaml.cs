using System.Windows;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.ViewModels.Properties;

namespace StencilPad.UI.Properties;

public partial class ShapePropertiesWindow : Window
{
    public ShapePropertiesViewModel ViewModel { get; }

    public ShapePropertiesWindow(IResourceService resourceService,
                                 IEnumerable<Shape> shapes)
    {
        InitializeComponent();

        ViewModel = new ShapePropertiesViewModel(shapes);
        DataContext = ViewModel;

        var capItems = new List<Geometry>()
        {
            resourceService.Get(GeometryResourceId.None),
            resourceService.Get(GeometryResourceId.Arrow0)
        };

        StartCapDropdown.Items = capItems;
        EndCapDropdown.Items = capItems;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
