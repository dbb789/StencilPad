using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using StencilPad.Common;
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
        var viewModel = new MainWindowViewModel { Project = project };
        var mainWindow = new MainWindow { DataContext = viewModel };
        
        ConfigureServices(services, mainWindow, viewModel, project);
        
        ServiceProvider = services.BuildServiceProvider();
        
        mainWindow.Show();

        ServiceProvider.GetRequiredService<AppController>().Initialize();
    }

    private static void ConfigureServices(IServiceCollection services,
                                          MainWindow mainWindow,
                                          MainWindowViewModel viewModel,
                                          Project project)
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
        
        services.AddSingleton<Project>(project);
        services.AddSingleton<MainWindowViewModel>(viewModel);
        services.AddSingleton<IAppConfigService, AppConfigService>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IImportExportService, ImportExportService>();
        services.AddSingleton<IPrintService, PrintService>();
        services.AddSingleton<IResourceService, ResourceService>();
        services.AddSingleton<ISettings, SettingsService>();

        services.AddSingleton<IOperationService, OperationService>();
        services.AddSingleton<SheetTabController.Factory>();
        services.AddSingleton<AppController>();
    }
}

