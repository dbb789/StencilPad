using System.Windows;
using StencilPad.Services;

namespace StencilPad.UI.Dialogs;

public class WpfDialogService : IDialogService
{
    private readonly Window _owner;

    public WpfDialogService(Window owner)
    {
        _owner = owner;
    }

    public string? ShowRenameDialog(string currentName)
    {
        var dialog = new SheetRenameDialog(currentName)
        {
            Owner = _owner
        };

        if (dialog.ShowDialog() == true)
        {
            return dialog.ViewModel.Name.Trim();
        }

        return null;
    }

    public bool ShowConfirmation(string message, string title)
    {
        return MessageBox.Show(_owner, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public void ShowWarning(string message, string title)
    {
        MessageBox.Show(_owner, message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public void ShowError(string message, string title)
    {
        MessageBox.Show(_owner, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
