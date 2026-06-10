using System.Collections.ObjectModel;
using System.Windows.Input;
using StencilPad.Models;

namespace StencilPad.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<SheetTabViewModel> Tabs { get; } = new();

    private Project? _project = null;
    public Project? Project
    {
        get => _project;
        set => SetProperty(ref _project, value);
    }

    private SheetTabViewModel? _selectedTab;
    public SheetTabViewModel? SelectedTab
    {
        get => _selectedTab;
        set => SetProperty(ref _selectedTab, value);
    }

    private string _statusText = string.Empty;
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private string _title = "StencilPad";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public ICommand NewProjectCommand { get; set; } = null!;
    public ICommand AddSheetCommand { get; set; } = null!;
    public ICommand RenameSheetCommand { get; set; } = null!;
    public ICommand DeleteSheetCommand { get; set; } = null!;
    public ICommand PrintCommand { get; set; } = null!;
    public ICommand ExitCommand { get; set; } = null!;
    public ICommand OpenProjectCommand { get; set; } = null!;
    public ICommand SaveProjectCommand { get; set; } = null!;
    public ICommand SaveProjectAsCommand { get; set; } = null!;
    public ICommand CopyCommand { get; set; } = null!;
    public ICommand CutCommand { get; set; } = null!;
    public ICommand PasteCommand { get; set; } = null!;
    public ICommand DeleteCommand { get; set; } = null!;
    public ICommand UndoCommand { get; set; } = null!;
    public ICommand RedoCommand { get; set; } = null!;
    public ICommand ImportImageCommand { get; set; } = null!;
    public ICommand ExportSvgCommand { get; set; } = null!;
    public ICommand ExportPngCommand { get; set; } = null!;
    
    public MainWindowViewModel()
    { }
}
