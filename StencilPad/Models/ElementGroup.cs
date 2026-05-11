using StencilPad.Spatial;

namespace StencilPad.Models;

public class ElementGroup : SheetElement<ElementGroup>
{
    public IEnumerable<ISheetElement> Children => _children;
    public override GroupHandleSet HandleSet { get; }

    private List<ISheetElement> _children;

    public ElementGroup(IEnumerable<ISheetElement> children)
    {
        _children = new(children.Select(c => c.DeepClone()));
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
        HandleSet.SetChildren(_children.Select(child => child.HandleSet));
        HandleSet.SetSelectedHandles(other.HandleSet.GetSelectedHandles());
    }

    public override ISheetElement DeepClone()
    {
        var clone = new ElementGroup(Children.Select(child => child.DeepClone()));

        clone.Id = Id;
        clone.HandleSet.SetSelectedHandles(HandleSet.GetSelectedHandles());

        return clone;
    }
}
