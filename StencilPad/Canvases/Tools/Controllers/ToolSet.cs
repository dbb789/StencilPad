using Microsoft.Extensions.DependencyInjection;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;

namespace StencilPad.Canvases.Tools.Controllers;

public class ToolSet
{
    public IEnumerable<IToolFactory> Tools => _tools;

    private List<IToolFactory> _tools;

    public ToolSet(SelectionTool.Factory selectionToolFactory,
                   EditTool.Factory editHandleSetToolFactory,
                   ShapeTool.Factory shapeToolFactory,
                   MarkerPathTool.Factory markerPathToolFactory,
                   RulerTool.Factory rulerToolFactory,
                   TextTool.Factory textToolFactory)
    {
        _tools = [
            selectionToolFactory,
            editHandleSetToolFactory,
            shapeToolFactory,
            markerPathToolFactory,
            rulerToolFactory,
            textToolFactory
            ];
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<SelectionTool.Factory>();
        services.AddSingleton<EditTool.Factory>();
        services.AddSingleton<ShapeTool.Factory>();
        services.AddSingleton<MarkerPathTool.Factory>();
        services.AddSingleton<RulerTool.Factory>();
        services.AddSingleton<TextTool.Factory>();

        services.AddTransient<EditToolOverlay>();
        services.AddSingleton<Factory<EditToolOverlay>>(sp => new(() => sp.GetRequiredService<EditToolOverlay>()));

        services.AddTransient<SelectionToolOverlay>();
        services.AddSingleton<Factory<SelectionToolOverlay>>(sp => new(() => sp.GetRequiredService<SelectionToolOverlay>()));

        services.AddTransient<ShapeToolOverlay>();
        services.AddSingleton<Factory<ShapeToolOverlay>>(sp => new(() => sp.GetRequiredService<ShapeToolOverlay>()));

        services.AddTransient<RulerToolOverlay>();
        services.AddSingleton<Factory<RulerToolOverlay>>(sp => new(() => sp.GetRequiredService<RulerToolOverlay>()));

        services.AddTransient<TextToolOverlay>();
        services.AddSingleton<Factory<TextToolOverlay>>(sp => new(() => sp.GetRequiredService<TextToolOverlay>()));
        
        services.AddSingleton<ToolSet>();
    }
}
