using System.Windows;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.ViewModels.Properties;

namespace StencilPad.UI.Properties;

public partial class MarkerPathPropertiesWindow : Window
{
    public MarkerPathPropertiesViewModel ViewModel { get; }

    public MarkerPathPropertiesWindow(IResourceService resourceService,
                                      Sheet sheet)
    {
        InitializeComponent();
        
        ViewModel = new MarkerPathPropertiesViewModel(resourceService, sheet);
        DataContext = ViewModel;
    }
    
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        ViewModel.Dispose();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
