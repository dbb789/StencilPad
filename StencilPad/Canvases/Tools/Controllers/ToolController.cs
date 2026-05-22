using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Services;
using StencilPad.ViewModels;

namespace StencilPad.Canvases.Tools.Controllers;

public class ToolController : IDisposable
{
    private readonly ToolSet _toolSet;
    private readonly ToolPanelViewModel _toolPanelViewModel;
    private readonly IModelPropertiesService _modelPropertiesService;
    private readonly Dictionary<IToolButton, ITool> _toolButtons;
    
    private ITool? _selectedTool;
    
    public ToolController(ToolSet toolSet,
                          ToolPanelViewModel toolPanelViewModel,
                          IModelPropertiesService modelPropertiesService)
    {
        _toolSet = toolSet;
        _toolPanelViewModel = toolPanelViewModel;
        _modelPropertiesService = modelPropertiesService;
        _toolButtons = [];
        _selectedTool = null;
        
        Initialize();
        
        _toolPanelViewModel.PropertyChanged += ToolPanelPropertyChanged;
    }

    public void Dispose()
    {
        ActivateTool(null);
        
        _toolPanelViewModel.PropertyChanged -= ToolPanelPropertyChanged;
        _toolPanelViewModel.Tools.Clear();
        _toolPanelViewModel.SelectedTool = null;
    }

    public void ActivateCurrentTool()
    {
        var selectedTool = _toolPanelViewModel.SelectedTool;

        ActivateTool(null);
        
        if (selectedTool != null && _toolButtons.TryGetValue(selectedTool, out var tool))
        {
            ActivateTool(tool);
        }
    }

    public void DeactivateCurrentTool()
    {
        ActivateTool(null);
    }
    
    private void Initialize()
    {
        foreach (var tool in _toolSet.Tools)
        {
            var button = CreateToolButton(tool.IconResource, tool.Tooltip);
            
            AttachToolButton(tool.Create(button), button);
        }
    }

    private void ToolPanelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ToolPanelViewModel.SelectedTool))
        {
            if (_toolPanelViewModel.SelectedTool is not null &&
                _toolButtons.TryGetValue(_toolPanelViewModel.SelectedTool, out var tool))
            {
                ActivateTool(tool);
            }
            else
            {
                ActivateTool(null);
            }
        }
    }

    private IToolButton CreateToolButton(string iconResource, string? tooltip = null)
    {
        var button = new ToolViewModel();

        button.Icon = Application.Current.Resources[iconResource] as BitmapImage;
        button.Tooltip = tooltip ?? "";
        
        _toolPanelViewModel.Tools.Add(button);
        _toolPanelViewModel.SelectedTool ??= button;

        return button;
    }

    private void AttachToolButton(ITool tool, IToolButton button)
    {
        _toolButtons[button] = tool;
    }

    private void ActivateTool(ITool? tool)
    {
        if (tool == _selectedTool)
        {
            return;
        }

        _selectedTool?.ToolEnd();
        _modelPropertiesService.CloseAll();

        _selectedTool = tool;
        _selectedTool?.ToolBegin();
    }
}
