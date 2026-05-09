using System.Collections.ObjectModel;

namespace StencilPad.ViewModels;

public class ToolPanelViewModel : ViewModelBase
{
    public ObservableCollection<ToolViewModel> Tools { get; }

    private ToolViewModel? _selectedTool;
    public ToolViewModel? SelectedTool
    {
        get => _selectedTool;
        set => SetProperty(ref _selectedTool, value);
    }
    
    public ToolPanelViewModel()
    {
        Tools = [];
    }
}
