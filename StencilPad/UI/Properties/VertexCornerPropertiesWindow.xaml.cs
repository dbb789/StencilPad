using System.Windows;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.ViewModels.Properties;

namespace StencilPad.UI.Properties;

public partial class VertexCornerPropertiesWindow : Window
{
    public VertexCornerPropertiesWindow(Sheet sheet,
                                        IOperationService operationService)
    {
        InitializeComponent();
        DataContext = new VertexCornerPropertiesViewModel(sheet, operationService);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
