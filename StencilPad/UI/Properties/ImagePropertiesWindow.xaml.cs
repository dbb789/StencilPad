using System.Windows;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.ViewModels.Properties;

namespace StencilPad.UI.Properties;

public partial class ImagePropertiesWindow : Window
{
    public ImagePropertiesViewModel ViewModel { get; }

    public ImagePropertiesWindow(Sheet sheet,
                                 ISettings settings,
                                 IOperationService operationService)
    {
        InitializeComponent();

        ViewModel = new ImagePropertiesViewModel(sheet,
                                                 settings,
                                                 operationService);
        DataContext = ViewModel;

        Loaded += (_, _) =>
        {
            OpacityField.DragBegin += ViewModel.DragBegin;
            OpacityField.DragEnd += ViewModel.DragEnd;
        };

        Unloaded += (_, _) =>
        {
            OpacityField.DragBegin -= ViewModel.DragBegin;
            OpacityField.DragEnd -= ViewModel.DragEnd;
        };
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
