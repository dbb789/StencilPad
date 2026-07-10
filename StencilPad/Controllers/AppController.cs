using StencilPad.ViewModels;
using StencilPad.UI;
using StencilPad.Services;

namespace StencilPad.Controllers;

public class AppController
{
    private readonly MainWindow _mainWindow;
    private readonly MainWindowController _mainWindowController;
    private readonly IDialogService _dialogService;
    
    public AppController(MainWindow mainWindow,
                         MainWindowViewModel mainWindowViewModel,
                         MainWindowController mainWindowController,
                         IDialogService dialogService)
    {
        _mainWindow = mainWindow;
        _mainWindow.DataContext = mainWindowViewModel;
        _mainWindowController = mainWindowController;
        _dialogService = dialogService;

        mainWindow.Closing += (_, e) =>
        {
            if (!ConfirmClose())
            {
                e.Cancel = true;
            }
        };
    }

    public void Initialize()
    {
        _mainWindowController.Initialize();
        _mainWindow.Show();
    }

    public bool ConfirmClose()
    {
        if (_mainWindowController.SaveState)
        {
            return true;
        }

        return _dialogService.ShowConfirmation(
            "You have unsaved changes. Are you sure you want to close without saving?",
            "Unsaved Changes",
            false);
    }
}
