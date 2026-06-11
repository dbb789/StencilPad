using StencilPad.Spatial;

namespace StencilPad.Services;

public interface IDialogService
{
    string? ShowRenameDialog(string currentName);
    (Unit Spacing, int Subdivisions)? ShowGridSettingsDialog(Unit currentSpacing,
                                                             int currentSubdivisions,
                                                             UnitSettings unitSettings);
    bool ShowConfirmation(string message, string title);
    void ShowWarning(string message, string title);
    void ShowError(string message, string title);
}
