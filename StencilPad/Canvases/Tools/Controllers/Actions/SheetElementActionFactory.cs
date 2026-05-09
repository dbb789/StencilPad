using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Controllers.Actions;

public static class SheetElementActionFactory
{
    public static IEnumerable<SheetElementAction> Create(IEnumerable<ISheetElement> elements)
    {
        var typeSet = new HashSet<Type>(elements.Select(e => e.GetType()));
        var list = new List<SheetElementAction>();
        
        foreach (var type in typeSet)
        {
            if (type.IsAssignableTo(typeof(IPolygonSheetElement)))
            {
                list.AddRange(PolygonSheetElementActions.Actions);
            }
        }

        return list;
    }
}
