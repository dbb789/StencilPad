using StencilPad.Models;
using StencilPad.Services;

namespace StencilPad.Canvases.Tools.Actions;

public class SheetElementAction<TInterface> : ISheetElementAction
{
    public Func<TInterface, bool>? Enabled { get; init; }
    public Action<TInterface>? Action { get; init;  }

    private readonly IOperationService _operationService;
    
    public SheetElementAction(IOperationService operationService)
    {
        _operationService = operationService;
    }
    
    public bool IsVisible(Sheet s, IEnumerable<ISheetElement> elements)
    {
        return elements.All(e => e is TInterface);
    }
    
    public bool IsEnabled(Sheet s, IEnumerable<ISheetElement> elements)
    {
        return elements.OfType<TInterface>().All(e => Enabled?.Invoke(e) ?? true);
    }

    public void Invoke(Sheet s, IEnumerable<ISheetElement> elements)
    {
        using var context = _operationService.CreateEditContext(s, elements);
        
        foreach (var element in elements.OfType<TInterface>())
        {
            Action?.Invoke(element);
        }
    }
}

public class SheetElementAction : SheetElementAction<ISheetElement>
{
    public SheetElementAction(IOperationService operationService)
        : base(operationService)
    {
        // ...
    }
}
