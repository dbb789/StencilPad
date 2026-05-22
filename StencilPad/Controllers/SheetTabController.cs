using Microsoft.Extensions.DependencyInjection;
using StencilPad.ViewModels;
using StencilPad.Canvases.Tools.Controllers;
using StencilPad.Canvases.UI;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.Canvases.Tools.Actions;

namespace StencilPad.Controllers;

public class SheetTabController : IDisposable
{
    public class Factory(IModelPropertiesService ModelPropertiesService,
                         IOperationService OperationService)
    {
        public SheetTabController Create(SheetTabViewModel tabViewModel)
        {
            return new(tabViewModel, OperationService, ModelPropertiesService);
        }
    }

    private readonly SheetTabViewModel _tabViewModel;
    private readonly IOperationService _operationService;
    private readonly IModelPropertiesService _modelPropertiesService;

    private SheetCanvas? _currentCanvas;
    private ToolController? _toolController;
    private ServiceProvider? _scopedServiceProvider;

    private SheetTabController(SheetTabViewModel tabViewModel,
                               IOperationService operationService,
                               IModelPropertiesService modelPropertiesService)
    {
        _tabViewModel = tabViewModel;
        _operationService = operationService;
        _modelPropertiesService = modelPropertiesService;

        _tabViewModel.CanvasAttached += CanvasAttached;
        _tabViewModel.CanvasDetached += CanvasDetached;
    }

    public void Dispose()
    {
        _tabViewModel.CanvasAttached -= CanvasAttached;
        _tabViewModel.CanvasDetached -= CanvasDetached;

        _toolController?.Dispose();
        _scopedServiceProvider?.Dispose();
    }

    private void CanvasAttached(SheetCanvas sheetCanvas)
    {
        if (_currentCanvas != sheetCanvas)
        {
            _toolController?.Dispose();
            _toolController = null;

            _currentCanvas = sheetCanvas;
        }

        if (_toolController is null)
        {
            _scopedServiceProvider?.Dispose();
            _scopedServiceProvider = CreateScopedServiceProvider(sheetCanvas);
            
            _toolController = _scopedServiceProvider.GetRequiredService<Factory<ToolController>>()
                .Create();
        }

        _tabViewModel.Viewport = sheetCanvas.Viewport;
        _toolController.ActivateCurrentTool();
    }

    private void CanvasDetached()
    {
        _tabViewModel.Viewport = null;
        _toolController?.DeactivateCurrentTool();
    }

    private ServiceProvider CreateScopedServiceProvider(SheetCanvas sheetCanvas)
    {
        var services = new ServiceCollection();

        ToolSet.ConfigureServices(services);
        SheetElementActionSet.ConfigureServices(services);
        SheetElementEditActionSet.ConfigureServices(services);

        sheetCanvas.ConfigureServices(services);

        services.AddSingleton<Sheet>(_tabViewModel.Sheet);
        services.AddSingleton<ToolPanelViewModel>(_tabViewModel.ToolPanelViewModel);
        services.AddSingleton<IOperationService>(_operationService);
        services.AddSingleton<IModelPropertiesService>(_modelPropertiesService);

        FactoryUtil.AddFactory<ToolController>(services);

        return services.BuildServiceProvider();
    }
}
