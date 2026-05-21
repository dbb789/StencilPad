using StencilPad.Spatial;

namespace StencilPad.Models;

public class ElementGroup : SheetElement<ElementGroup>
{
    public IEnumerable<ISheetElement> Children => _children;

    private List<ISheetElement> _children;
    private GroupHandleSource _groupHandleSource;

    public event Action? ChildrenChanged;
    
    public ElementGroup()
    {
        _children = new();
        _groupHandleSource = new();
        SetHandleSource(_groupHandleSource);
    }
    
    public ElementGroup(IEnumerable<ISheetElement> children)
    {
        _children = new(children.Select(c => c.DeepClone()));
        _groupHandleSource = new(_children);
        SetHandleSource(_groupHandleSource);
    }

    public override void MirrorX(Unit centerY)
    {
        Transform = Transform with 
        { 
            Position = Transform.Position with { Y = (centerY * 2) - Transform.Position.Y },
            Angle = -Transform.Angle
        };

        foreach (var child in _children)
        {
            child.MirrorX(Unit.Zero);
        }
    }

    public override void MirrorY(Unit centerX)
    {
        Transform = Transform with 
        { 
            Position = Transform.Position with { X = (centerX * 2) - Transform.Position.X },
            Angle = -Transform.Angle
        };

        foreach (var child in _children)
        {
            child.MirrorY(Unit.Zero);
        }
    }
    
    public override void Translate(Unit2D delta)
    {
        Transform = Transform with { Position = Transform.Position + delta };
    }

    public override void NormalizePosition()
    {
        if (_children.Count == 0)
        {
            return;
        }

        var sum = Unit2D.Zero;

        foreach (var child in _children)
        {
            sum += child.Transform.Position;
        }

        var midpoint = sum / _children.Count;

        foreach (var child in _children)
        {
            child.Transform = child.Transform with { Position = child.Transform.Position - midpoint };
        }

        Transform = Transform with { Position = Transform.Position + Transform.Rotate(midpoint) };
    }

    public override UnitBounds GetBounds()
    {
        UnitBounds? bounds = null;

        foreach (var child in _children)
        {
            bounds = UnitBounds.Union(bounds, child.GetBounds().ApplyTransform(child.Transform));
        }

        return bounds ?? UnitBounds.Empty;
    }

    public override void AssignFrom(ElementGroup other)
    {
        _children = new(other.Children.Select(child => child.DeepClone()));
        _groupHandleSource.SetChildren(_children);

        Transform = other.Transform;

        ChildrenChanged?.Invoke();
    }

    public override ISheetElement DeepClone()
    {
        var clone = new ElementGroup();

        clone.Id = Id;
        clone.AssignFrom(this);

        return clone;
    }
}
