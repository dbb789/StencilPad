using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using StencilPad.Export;
using StencilPad.Services;
using StencilPad.ViewModels;

namespace StencilPad.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        var sheet = (DataContext as MainWindowViewModel)?.SelectedTab?.Sheet;
        if (sheet is null) return;

        var dialog = new SaveFileDialog
        {
            Title = "Export PNG",
            Filter = "PNG Image|*.png",
            FileName = sheet.Name
        };

        if (dialog.ShowDialog(this) != true) return;

        var resourceService = (IResourceService)App.ServiceProvider.GetService(typeof(IResourceService))!;
        PngExporter.Export(sheet, dialog.FileName, resourceService);
    }

    private SheetTab? GetActiveSheetTab()
    {
        if (SheetTabs.SelectedIndex < 0)
        {
            return null;
        }
        
        var tabItem = SheetTabs.ItemContainerGenerator.ContainerFromIndex(SheetTabs.SelectedIndex) as System.Windows.Controls.TabItem;
        
        return FindVisualChild<SheetTab>(tabItem);
    }

    private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
    {
        if (parent == null)
        {
            return null;
        }
        
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); ++i)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T t)
            {
                return t;
            }
            
            var result = FindVisualChild<T>(child);

            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
}
