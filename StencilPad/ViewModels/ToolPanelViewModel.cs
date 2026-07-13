using System.Collections.ObjectModel;
using System.Windows.Input;
using StencilPad.Common;

namespace StencilPad.ViewModels;

public class ToolPanelViewModel : ViewModelBase
{
    public ObservableCollection<ToolViewModel> Tools { get; }

    public ICommand SelectToolCommand { get; set; } = null!;

    private ToolViewModel? _selectedTool;
    public ToolViewModel? SelectedTool
    {
        get => _selectedTool;
        set => SetProperty(ref _selectedTool, value);
    }
    
    public ToolPanelViewModel()
    {
        Tools = [];

        SelectToolCommand = new RelayCommand<ToolViewModel>(SelectTool);
    }

    private void SelectTool(ToolViewModel tool)
    {
        SelectedTool = tool;
    }
}
