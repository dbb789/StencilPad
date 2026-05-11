using Microsoft.Extensions.DependencyInjection;
using StencilPad.Canvases.Tools.Controllers.Actions;

namespace StencilPad.Canvases.Tools.Actions;

public class SheetElementEditActionSet
{
    public IEnumerable<ISheetElementAction?> Actions { get; }

    public SheetElementEditActionSet(PolygonSheetElementEditActionSet polygonActions)
    {
        Actions = [.. polygonActions.Actions];
    }
    
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<PolygonSheetElementEditActionSet>();
        services.AddSingleton<SheetElementEditActionSet>();
    }
}
