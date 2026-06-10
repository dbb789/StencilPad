using System.Windows;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.ViewModels.Properties;

namespace StencilPad.UI.Properties;

public partial class VertexCornerPropertiesWindow : Window
{
    public VertexCornerPropertiesWindow(Sheet sheet,
                                        ISettings settings,
                                        IOperationService operationService)
    {
        InitializeComponent();
        DataContext = new VertexCornerPropertiesViewModel(sheet, settings, operationService);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
