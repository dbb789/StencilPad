using StencilPad.ViewModels;
using StencilPad.Canvases.Tools.Controllers;
using StencilPad.Canvases.UI;
using StencilPad.Services;

namespace StencilPad.Controllers;

public class SheetTabController : IDisposable
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
        
        _tabViewModel.CanvasAttached += CanvasAttached;
        _tabViewModel.CanvasDetached += CanvasDetached;
    }

    public void Dispose()
    {
        _tabViewModel.CanvasAttached -= CanvasAttached;
        _tabViewModel.CanvasDetached -= CanvasDetached;

        _toolController?.Dispose();
    }

    private void CanvasAttached(SheetCanvas sheetCanvas)
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

        _tabViewModel.Viewport = sheetCanvas.Viewport;
        _toolController.ActivateCurrentTool();
    }

    private void CanvasDetached()
    {
        _tabViewModel.Viewport = null;
        _toolController?.DeactivateCurrentTool();
    }
}
