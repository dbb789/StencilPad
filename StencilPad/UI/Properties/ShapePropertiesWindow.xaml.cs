using System.Windows;
using StencilPad.Models;
using StencilPad.ViewModels.Properties;

namespace StencilPad.UI.Properties;

public partial class ShapePropertiesWindow : Window
{
    public ShapePropertiesViewModel ViewModel { get; }

    public ShapePropertiesWindow(IEnumerable<Shape> shapes)
    {
        InitializeComponent();
        ViewModel = new ShapePropertiesViewModel(shapes);
        DataContext = ViewModel;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
