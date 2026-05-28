using System.Windows;
using StencilPad.Models;
using StencilPad.ViewModels.Properties;

namespace StencilPad.UI.Properties;

public partial class TextPropertiesWindow : Window
{
    public TextPropertiesViewModel ViewModel { get; }

    public TextPropertiesWindow(Sheet sheet)
    {
        InitializeComponent();

        ViewModel = new TextPropertiesViewModel(sheet);
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
