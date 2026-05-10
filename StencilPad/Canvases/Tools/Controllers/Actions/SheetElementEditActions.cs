using Microsoft.Extensions.DependencyInjection;
using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Controllers.Actions;

public class SheetElementEditActions
{
    private PolygonSheetElementEditActions _polygonActions;

    public SheetElementEditActions(PolygonSheetElementEditActions polygonActions)
    {
        _polygonActions = polygonActions;
    }
    
    public IEnumerable<ISheetElementAction> Create(IEnumerable<ISheetElement> elements)
    {
        var typeSet = new HashSet<Type>(elements.Select(e => e.GetType()));
        var list = new List<ISheetElementAction>();
        
        foreach (var type in typeSet)
        {
            if (type.IsAssignableTo(typeof(IPolygonSheetElement)))
            {
                list.AddRange(_polygonActions.Actions);
            }
        }

        return list;
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<PolygonSheetElementEditActions>();
        services.AddSingleton<SheetElementEditActions>();
    }
}
