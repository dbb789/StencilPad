using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Actions;

public class SheetElementAction<TInterface> : ISheetElementAction
{
    public string Name { get; init;  } = "";

    public Func<TInterface, bool>? Enabled { get; init; }
    public Action<TInterface>? Action { get; init;  }

    public bool IsVisible(Sheet _, IEnumerable<ISheetElement> elements)
    {
        return elements.All(e => e is TInterface);
    }
    
    public bool IsEnabled(Sheet _, IEnumerable<ISheetElement> elements)
    {
        return elements.OfType<TInterface>().All(e => Enabled?.Invoke(e) ?? true);
    }

    public void Invoke(Sheet _, IEnumerable<ISheetElement> elements)
    {
        foreach (var element in elements.OfType<TInterface>())
        {
            Action?.Invoke(element);
        }
    }
}

public class SheetElementAction : SheetElementAction<ISheetElement>
{
    // ...
}
