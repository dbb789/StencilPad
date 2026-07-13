using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    public class Factory(ISettings Settings,
                         IResourceService ResourceService,
                         IModelPropertiesService ModelPropertiesService,
                         IOperationService OperationService)
    {
        public SheetTabController Create(SheetTabViewModel tabViewModel)
        {
            return new(tabViewModel,
                       Settings,
                       ResourceService,
                       OperationService,
                       ModelPropertiesService);
        }
    }

    private readonly ISettings _settings;
    private readonly SheetTabViewModel _tabViewModel;
    private readonly IResourceService _resourceService;
    private readonly IOperationService _operationService;
    private readonly IModelPropertiesService _modelPropertiesService;
    private readonly HintService _hintService;
    
    private SheetCanvas? _currentCanvas;
    private ToolController? _toolController;
    private ServiceProvider? _scopedServiceProvider;

    private SheetTabController(SheetTabViewModel tabViewModel,
                               ISettings settings,
                               IResourceService resourceService,
                               IOperationService operationService,
                               IModelPropertiesService modelPropertiesService)
    {
        _tabViewModel = tabViewModel;
        _settings = settings;
        _resourceService = resourceService;
        _operationService = operationService;
        _modelPropertiesService = modelPropertiesService;
        _hintService = new HintService();
        
        _hintService.HintChanged += OnHintTextChanged;
        _tabViewModel.CanvasAttached += OnCanvasAttached;
        _tabViewModel.CanvasDetached += OnCanvasDetached;

        tabViewModel.ToggleSelectCommand = new RelayCommand(() =>
        {
            _toolController?.ToggleSelect();
        });
    }

    public void Dispose()
    {
        _hintService.HintChanged += OnHintTextChanged;
        _tabViewModel.CanvasAttached -= OnCanvasAttached;
        _tabViewModel.CanvasDetached -= OnCanvasDetached;

        _toolController?.Dispose();
        _scopedServiceProvider?.Dispose();
    }

    private void OnHintTextChanged(string text)
    {
        _tabViewModel.HintText = text;
    }

    private void OnCanvasAttached(SheetCanvas sheetCanvas)
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

    private void OnCanvasDetached()
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
        services.AddSingleton<ISettings>(_settings);
        services.AddSingleton<IResourceService>(_resourceService);
        services.AddSingleton<IOperationService>(_operationService);
        services.AddSingleton<IModelPropertiesService>(_modelPropertiesService);
        services.AddSingleton<IHintService>(_hintService);

        services.AddLogging(builder =>
        {
            builder.AddDebug();
        });

        FactoryUtil.AddFactory<ToolController>(services);

        return services.BuildServiceProvider();
    }
}
