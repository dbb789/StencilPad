using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using StencilPad.Controllers;
using StencilPad.Models;
using StencilPad.UI;
using StencilPad.UI.Dialogs;
using StencilPad.ViewModels;
using StencilPad.Services;

namespace StencilPad;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        var project = new Project();
        var viewModel = new MainWindowViewModel();
        var mainWindow = new MainWindow() { DataContext = viewModel };
        
        ConfigureServices(services, mainWindow);
        
        ServiceProvider = services.BuildServiceProvider();;
        
        var controllerFactory = ServiceProvider.GetRequiredService<AppController.Factory>();
        var controller = controllerFactory.Create(project, viewModel);
        
        mainWindow.Show();

        controller.Initialize();
    }

    private static void ConfigureServices(IServiceCollection services, MainWindow mainWindow)
    {
        services.AddSingleton<IDialogService>(sp => 
        {
            return new WpfDialogService(mainWindow);
        });

        services.AddSingleton<IModelPropertiesService>(sp =>
        {
            var resourceService = sp.GetRequiredService<IResourceService>();
            var operationService = sp.GetRequiredService<IOperationService>();

            return new WpfModelPropertiesService(mainWindow,
                                                 resourceService,
                                                 operationService);
        });

        services.AddSingleton<IPrintService, PrintService>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IResourceService, ResourceService>();

        services.AddSingleton<IOperationService, OperationService>();
        services.AddSingleton<SheetTabController.Factory>();
        services.AddSingleton<AppController.Factory>();
    }
}

