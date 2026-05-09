using System.Windows;
using StencilPad.ViewModels.Dialogs;

namespace StencilPad.UI.Dialogs;

public partial class SheetRenameDialog : Window
{
    public SheetRenameDialogViewModel ViewModel { get; }

    public SheetRenameDialog(string currentName)
    {
        InitializeComponent();
        ViewModel = new SheetRenameDialogViewModel(currentName);
        DataContext = ViewModel;

        Loaded += (s, e) =>
        {
            NameBox.Focus();
            NameBox.SelectAll();
        };
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ViewModel.Name))
        {
            MessageBox.Show("Sheet name cannot be empty.", "Rename Sheet",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
