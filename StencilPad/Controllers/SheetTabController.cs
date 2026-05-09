using StencilPad.ViewModels;
using StencilPad.Canvases.Tools.Controllers;
using StencilPad.Services;

namespace StencilPad.Controllers;

public class SheetTabController
{
    public class Factory(ToolSet ToolSet, IModelPropertiesService ModelPropertiesService)
    {
        public SheetTabController Create(SheetTabViewModel tabViewModel)
        {
            return new(tabViewModel, ToolSet, ModelPropertiesService);
        }
    }

    private readonly SheetTabViewModel _tabViewModel;
    private readonly ToolSet _toolSet;
    private readonly IModelPropertiesService _modelPropertiesService;
    
    private ToolController? _toolController;

    private SheetTabController(SheetTabViewModel tabViewModel,
                               ToolSet toolSet,
                               IModelPropertiesService modelPropertiesService)
    {
        _tabViewModel = tabViewModel;
        _toolSet = toolSet;
        _modelPropertiesService = modelPropertiesService;
        
        tabViewModel.CanvasAttached += sheetCanvas =>
        {
            if (sheetCanvas != _toolController?.ToolContext)
            {
                _toolController?.Dispose();
                _toolController = null;
            }
            
            _toolController ??= new ToolController(_toolSet,
                                                   _tabViewModel.ToolPanelViewModel,
                                                   _tabViewModel.Sheet,
                                                   sheetCanvas,
                                                   _modelPropertiesService);

            _toolController.ActivateCurrentTool();
        };

        tabViewModel.CanvasDetached += () =>
        {
            _toolController?.DeactivateCurrentTool();
        };
    }
}
