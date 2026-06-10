using System.Windows;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.ViewModels.Properties;

namespace StencilPad.UI.Properties;

public partial class TextPropertiesWindow : Window
{
    public TextPropertiesViewModel ViewModel { get; }

    public TextPropertiesWindow(Sheet sheet,
                                ISettings settings,
                                IOperationService operationService)
    {
        InitializeComponent();

        ViewModel = new TextPropertiesViewModel(sheet,
                                                settings,
                                                operationService);
        DataContext = ViewModel;
        
        Loaded += (_, _) =>
        {
            ColorField.DragBegin += ViewModel.DragBegin;
            ColorField.DragEnd += ViewModel.DragEnd;
        };

        Unloaded += (_, _) =>
        {
            ColorField.DragBegin -= ViewModel.DragBegin;
            ColorField.DragEnd -= ViewModel.DragEnd;
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
