using StencilPad.Canvases.Tools.Common;
using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Actions;

public class MultiSheetElementAction<TInterface> : ISheetElementAction
{
    public string Name { get; init;  } = "";

    public Func<IEnumerable<TInterface>, bool>? Enabled { get; init; }
    public Action<IToolContext, Sheet, IEnumerable<TInterface>>? Action { get; init;  }

    public bool IsVisible(IToolContext c, Sheet s, IEnumerable<ISheetElement> elements)
    {
        return elements.All(e => e is TInterface);
    }
    
    public bool IsEnabled(IToolContext c, Sheet s, IEnumerable<ISheetElement> elements)
    {
        return Enabled?.Invoke(elements.OfType<TInterface>()) ?? true;
    }

    public void Invoke(IToolContext context, Sheet sheet, IEnumerable<ISheetElement> elements)
    {
        Action?.Invoke(context, sheet, elements.OfType<TInterface>());
    }
}

public class MultiSheetElementAction : MultiSheetElementAction<ISheetElement>
{
    // ...
}
