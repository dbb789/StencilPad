using System.Windows;
using StencilPad.Models;
using StencilPad.ViewModels.Properties;

namespace StencilPad.UI.Properties;

public partial class RulerPropertiesWindow : Window
{
    public RulerPropertiesViewModel ViewModel { get; }

    public RulerPropertiesWindow(Sheet sheet)
    {
        InitializeComponent();

        ViewModel = new RulerPropertiesViewModel(sheet);
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
