namespace StencilPad.ViewModels.Dialogs;

public class SheetRenameDialogViewModel : ViewModelBase
{
    private string _name;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public SheetRenameDialogViewModel(string name)
    {
        _name = name;
    }
}
