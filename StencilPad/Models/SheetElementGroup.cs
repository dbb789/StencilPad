using StencilPad.Spatial;

namespace StencilPad.Models;

public class ElementGroup : SheetElement<ElementGroup>
{
    public IEnumerable<ISheetElement> Children => _children;
    public override GroupHandleSet HandleSet { get; }

    private List<ISheetElement> _children;

    public ElementGroup(IEnumerable<ISheetElement> children)
    {
        _children = new(children);
        HandleSet = new(_children.Select(child => child.HandleSet));
    }

    public override void Translate(Unit2D delta)
    {
        foreach (var child in _children)
        {
            child.Translate(delta);
        }
    }

    public override void AssignFrom(ElementGroup other)
    {
        _children = new(other.Children.Select(child => child.DeepClone()));
    }

    public override ISheetElement DeepClone()
    {
        return new ElementGroup(Children.Select(child => child.DeepClone()));
    }
}
