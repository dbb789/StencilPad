using StencilPad.Spatial;

namespace StencilPad.Models;

public abstract class SheetElement<TSelf> : SheetElement where TSelf : SheetElement<TSelf>
{
    public abstract void AssignFrom(TSelf other);

    public override void AssignFromElement(ISheetElement other)
    {
        if (other is not TSelf tOther)
        {
            throw new ArgumentException($"Expected element of type {typeof(TSelf).Name} but got {other.GetType().Name}");
        }

        AssignFrom(tOther);
    }
}

public abstract class SheetElement : ModelBase, ISheetElement
{
    public abstract void Translate(Unit2D delta);
    public abstract void AssignFromElement(ISheetElement other);
    public abstract ISheetElement DeepClone();
}
