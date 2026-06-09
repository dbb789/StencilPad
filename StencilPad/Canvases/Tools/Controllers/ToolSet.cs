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
                   RectTool.Factory rectToolFactory,
                   CircleTool.Factory circleToolFactory,
                   MarkerPathTool.Factory markerPathToolFactory,
                   RulerTool.Factory rulerToolFactory,
                   TextTool.Factory textToolFactory)
    {
        _tools = [
            selectionToolFactory,
            editHandleSetToolFactory,
            straightLineToolFactory,
            curvedLineToolFactory,
            rectToolFactory,
            circleToolFactory,
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
        services.AddSingleton<RectTool.Factory>();
        services.AddSingleton<CircleTool.Factory>();
        services.AddSingleton<MarkerPathTool.Factory>();
        services.AddSingleton<RulerTool.Factory>();
        services.AddSingleton<TextTool.Factory>();

        FactoryUtil.AddFactory<EditToolOverlay>(services);
        FactoryUtil.AddFactory<SelectionToolOverlay>(services);
        FactoryUtil.AddFactory<LineToolOverlay<Shape>>(services);
        FactoryUtil.AddFactory<LineToolOverlay<MarkerPath>>(services);
        FactoryUtil.AddFactory<RectToolOverlay<Shape>>(services);
        FactoryUtil.AddFactory<CircleToolOverlay<Shape>>(services);
        FactoryUtil.AddFactory<RulerToolOverlay>(services);
        FactoryUtil.AddFactory<TextToolOverlay>(services);
        
        services.AddSingleton<ToolSet>();
    }
}
