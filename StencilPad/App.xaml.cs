using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using StencilPad.Canvases.Tools.Controllers;
using StencilPad.Controllers;
using StencilPad.Models;
using StencilPad.UI;
using StencilPad.UI.Dialogs;
using StencilPad.ViewModels;
using StencilPad.Services;

namespace StencilPad;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        var project = new Project();
        var viewModel = new MainWindowViewModel();
        var mainWindow = new MainWindow() { DataContext = viewModel };
        
        ConfigureServices(services, mainWindow);
        
        var serviceProvider = services.BuildServiceProvider();
        var controllerFactory = serviceProvider.GetRequiredService<AppController.Factory>();
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
            var operationService = sp.GetRequiredService<IOperationService>();
            return new WpfModelPropertiesService(mainWindow, operationService);
        });

        services.AddSingleton<IPrintService, PrintService>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<IFileService, FileService>();

        ToolSet.ConfigureServices(services);
        
        services.AddSingleton<IOperationService, OperationService>();
        services.AddSingleton<SheetTabController.Factory>();
        services.AddSingleton<AppController.Factory>();
    }
}

