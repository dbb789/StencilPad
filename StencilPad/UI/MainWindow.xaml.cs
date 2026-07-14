using System.Windows;

namespace StencilPad.UI;

public partial class MainWindow : Window, IWpfDialogParent
{
    public Window Window => this;
    
    public MainWindow()
    {
        InitializeComponent();
    }
}
