using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StencilPad.Canvases.Common;
using StencilPad.Common;
using StencilPad.Controllers;
using StencilPad.Export;
using StencilPad.Models;
using StencilPad.Models.Resolvers;
using StencilPad.Rendering;
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

        ConfigureServices(services);
        
        ServiceProvider = services.BuildServiceProvider();
        
        var appController = ServiceProvider.GetRequiredService<AppController>();
        
        appController.Initialize();

        if (e.Args.Length > 0)
        {
            appController.OpenFile(e.Args[0]);
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {        
        services.AddSingleton<Project>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<IWpfDialogParent>(x => x.GetService<MainWindow>()!);
        services.AddSingleton<IDialogService, WpfDialogService>();
        services.AddSingleton<IModelPropertiesService, WpfModelPropertiesService>();
        services.AddSingleton<IAppConfigService, AppConfigService>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<PngExporter>();
        services.AddSingleton<SvgExporter>();
        services.AddSingleton<IImportExportService, ImportExportService>();
        services.AddSingleton<IPrintService, PrintService>();
        services.AddSingleton<IResourceService, ResourceService>();
        services.AddSingleton<IResourceSet>(x => x.GetService<IResourceService>()!);
        services.AddSingleton<ISettings, SettingsService>();
        services.AddSingleton<IOperationService, OperationService>();
        services.AddSingleton<HandleMap.Factory>();
        services.AddSingleton<SheetResolver.Factory>();
        services.AddSingleton<SheetRenderer.Factory>();
        services.AddSingleton<SheetTabController.Factory>();
        services.AddSingleton<MainWindowController>();
        services.AddSingleton<AppController>();

        services.AddLogging(builder =>
        {
            builder.AddDebug();
        });
    }
}

