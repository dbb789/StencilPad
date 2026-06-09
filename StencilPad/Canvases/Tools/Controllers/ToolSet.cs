using Microsoft.Extensions.DependencyInjection;
using StencilPad.Canvases.Tools.Overlays;
using StencilPad.Common;
using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Controllers;

public class ToolSet
{
    public IEnumerable<IToolFactory> Tools => _tools;

    private List<IToolFactory> _tools;

    public ToolSet(SelectionTool.Factory selectionToolFactory,
                   EditTool.Factory editHandleSetToolFactory,
                   StraightLineTool.Factory straightLineToolFactory,
                   CurvedLineTool.Factory curvedLineToolFactory,
                   MarkerPathTool.Factory markerPathToolFactory,
                   RulerTool.Factory rulerToolFactory,
                   TextTool.Factory textToolFactory)
    {
        _tools = [
            selectionToolFactory,
            editHandleSetToolFactory,
            straightLineToolFactory,
            curvedLineToolFactory,
            markerPathToolFactory,
            rulerToolFactory,
            textToolFactory
            ];
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<SelectionTool.Factory>();
        services.AddSingleton<EditTool.Factory>();
        services.AddSingleton<StraightLineTool.Factory>();
        services.AddSingleton<CurvedLineTool.Factory>();
        services.AddSingleton<MarkerPathTool.Factory>();
        services.AddSingleton<RulerTool.Factory>();
        services.AddSingleton<TextTool.Factory>();

        FactoryUtil.AddFactory<EditToolOverlay>(services);
        FactoryUtil.AddFactory<SelectionToolOverlay>(services);
        FactoryUtil.AddFactory<PolygonToolOverlay<Shape>>(services);
        FactoryUtil.AddFactory<PolygonToolOverlay<MarkerPath>>(services);
        FactoryUtil.AddFactory<RulerToolOverlay>(services);
        FactoryUtil.AddFactory<TextToolOverlay>(services);
        
        services.AddSingleton<ToolSet>();
    }
}
