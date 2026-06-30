using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public abstract class SheetElementResolver : ISheetElementResolver
{
    public ISheetElement Element { get; private init; }

    public event Action? OutlineChanged;

    public SheetElementResolver(ISheetElement element)
    {
        Element = element;
    }

    public abstract void Dispose();

    public virtual UnitBounds GetOutlineBounds(UnitTransform transform)
    {
        return Element.GetTransformedBounds(transform);
    }

    public UnitBounds GetOutlineBounds()
    {
        return GetOutlineBounds(Element.Transform);
    }

    public virtual bool OutlineContainsPoint(Unit2D point)
    {
        return GetOutlineBounds().Contains(point);
    }

    public abstract void Attach(IModelWalker walker);
    public abstract void Detach();

    protected void InvokeOutlineChanged()
    {
        OutlineChanged?.Invoke();
    }
}
