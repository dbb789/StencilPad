using StencilPad.Spatial;

namespace StencilPad.Services;

public interface IDialogService
{
    string? ShowRenameDialog(string currentName);
    bool ShowConfirmation(string message, string title);
    void ShowWarning(string message, string title);
    void ShowError(string message, string title);
}
