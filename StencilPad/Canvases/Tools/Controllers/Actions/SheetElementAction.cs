using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Controllers.Actions;

public class SheetElementAction<TInterface> : SheetElementAction
{
    public Func<TInterface, bool>? Enabled { get; init; }
    public Action<TInterface>? Action { get; init;  }

    protected override bool IsVisible(ISheetElement element)
    {
        return element is TInterface;
    }
    
    protected override bool IsEnabled(ISheetElement element)
    {
        if (element is not TInterface tInterface)
        {
            return false;
        }
        
        return Enabled?.Invoke(tInterface) ?? true;
    }

    protected override void Invoke(ISheetElement element)
    {
        if (element is not TInterface tInterface)
        {
            return;
        }

        Action?.Invoke(tInterface);
    }
}

public abstract class SheetElementAction
{
    public string Name { get; init;  } = "";

    protected abstract bool IsVisible(ISheetElement element);
    protected abstract bool IsEnabled(ISheetElement element);
    protected abstract void Invoke(ISheetElement element);

    public bool IsVisible(IEnumerable<ISheetElement> elements)
    {
        return elements.All(IsVisible);
    }
    
    public bool IsEnabled(IEnumerable<ISheetElement> elements)
    {
        return elements.All(IsEnabled);
    }

    public void Invoke(IEnumerable<ISheetElement> elements)
    {
        foreach (var element in elements)
        {
            Invoke(element);
        }
    }
}
