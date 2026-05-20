using System.Windows;
using StencilPad.Models;
using StencilPad.ViewModels.Properties;

namespace StencilPad.UI.Properties;

public partial class MarkerPathPropertiesWindow : Window
{
    public MarkerPathPropertiesViewModel ViewModel { get; }

    public MarkerPathPropertiesWindow(IEnumerable<MarkerPath> MarkerPaths)
    {
        InitializeComponent();
        ViewModel = new MarkerPathPropertiesViewModel(MarkerPaths);
        DataContext = ViewModel;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
