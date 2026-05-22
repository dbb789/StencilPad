using Microsoft.Extensions.DependencyInjection;

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
        services.AddSingleton<ToolSet>();
    }
}
