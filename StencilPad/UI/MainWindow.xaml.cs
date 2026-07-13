using System.Windows;
using System.Windows.Media;

namespace StencilPad.UI;

public partial class MainWindow : Window, IWpfDialogParent
{
    public Window Window => this;
    
    public MainWindow()
    {
        InitializeComponent();
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
