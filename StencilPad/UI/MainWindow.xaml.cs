using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using StencilPad.ViewModels;

namespace StencilPad.UI;

public partial class MainWindow : Window, IWpfDialogParent
{
    public Window Window => this;
    
    public MainWindow()
    {
        InitializeComponent();
    }

    private void TabItemDrag(object sender, MouseEventArgs e)
    {
        var tabItem = e.Source as TabItem;

        if (tabItem is null)
        {
            return;
        }

        if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
        {
            DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
        }
    }

    private void TabItemDrop(object sender, DragEventArgs e)
    {
        var tabItemTarget = (e.Source as TabItem)?.DataContext as SheetTabViewModel;
        var tabItemSource = (e.Data.GetData(typeof(TabItem)) as TabItem)?.DataContext as SheetTabViewModel;

        if (tabItemSource is null || tabItemTarget is null)
        {
            return;
        }
        
        int sourceIndex = SheetTabs.Items.IndexOf(tabItemSource);
        int targetIndex = SheetTabs.Items.IndexOf(tabItemTarget);

        if (sourceIndex == targetIndex)
        {
            return;
        }
        
        var viewModel = DataContext as MainWindowViewModel;

        if (viewModel is null)
        {
            return;
        }

        viewModel.SheetTabReordered?.Invoke(sourceIndex, targetIndex);
    }
}
