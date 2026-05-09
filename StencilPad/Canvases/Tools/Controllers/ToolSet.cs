using Microsoft.Extensions.DependencyInjection;

namespace StencilPad.Canvases.Tools.Controllers;

public class ToolSet
{
    public IEnumerable<IToolFactory> Tools => _tools;

    private List<IToolFactory> _tools;

    public ToolSet(SelectionTool.Factory selectionToolFactory,
                   EditHandleSetTool.Factory editHandleSetToolFactory,
                   ShapeTool.Factory shapeToolFactory,
                   MarkerPathTool.Factory markerPathToolFactory,
                   RulerTool.Factory rulerToolFactory)
    {
        _tools = [
            selectionToolFactory,
            editHandleSetToolFactory,
            shapeToolFactory,
            markerPathToolFactory,
            rulerToolFactory
            ];
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<SelectionTool.Factory>();
        services.AddSingleton<EditHandleSetTool.Factory>();
        services.AddSingleton<ShapeTool.Factory>();
        services.AddSingleton<MarkerPathTool.Factory>();
        services.AddSingleton<RulerTool.Factory>();
        services.AddSingleton<ToolSet>();
    }
}
