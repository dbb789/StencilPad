using StencilPad.Spatial;

namespace StencilPad.Models;

public class ElementGroup : SheetElement<ElementGroup>
{
    public IEnumerable<ISheetElement> Children => _children;

    private List<ISheetElement> _children;
    private GroupHandleSource<ISheetElement> _groupHandleSource;

    public event Action? ChildrenChanged;

    private void SubscribeChildren(IEnumerable<ISheetElement> children)
    {
        foreach (var child in children)
        {
            child.GeometryChanged += OnChildGeometryChanged;
            child.TransformChanged += OnChildTransformChanged;
        }
    }

    private void UnsubscribeChildren(IEnumerable<ISheetElement> children)
    {
        foreach (var child in children)
        {
            child.GeometryChanged -= OnChildGeometryChanged;
            child.TransformChanged -= OnChildTransformChanged;
        }
    }

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
        SubscribeChildren(_children);
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
    
    public override void NormalizePosition()
    {
        if (_children.Count == 0)
        {
            return;
        }

        foreach (var child in _children)
        {
            child.NormalizePosition();
        }
        
        var midpoint = Unit2D.Zero;

        foreach (var child in _children)
        {
            midpoint += child.Transform.Position;
        }

        midpoint /= _children.Count;

        foreach (var child in _children)
        {
            child.Transform = child.Transform with { Position = child.Transform.Position - midpoint };
        }

        Transform = Transform with { Position = Transform.Position + Transform.Rotate(midpoint) };
    }

    public override UnitBounds GetTransformedBounds(UnitTransform transform)
    {
        UnitBounds? bounds = null;

        foreach (var child in _children)
        {
            bounds = UnitBounds.Union(bounds, child.GetTransformedBounds(transform * child.Transform));
        }

        return bounds ?? UnitBounds.Empty;
    }

    private void OnChildGeometryChanged(ISheetElement element)
    {
        FireGeometryChanged();
    }

    private void OnChildTransformChanged(ISheetElement _)
    {
        FireGeometryChanged();
    }

    public override void SetTransformedBounds(UnitBounds newBounds, UnitTransform transform)
    {
        var oldBounds = GetTransformedBounds(transform);

        foreach (var child in _children)
        {
            var childCombinedTransform = transform * child.Transform;
            var oldChildWorldBounds = child.GetTransformedBounds(childCombinedTransform);
            var newChildWorldBounds = RemapWorldBounds(oldChildWorldBounds, oldBounds, newBounds);
            child.SetTransformedBounds(newChildWorldBounds, childCombinedTransform);
        }
    }

    private static Unit2D RemapWorldPoint(Unit2D worldPt, UnitBounds oldBounds, UnitBounds newBounds)
    {
        var oldSize = oldBounds.Size;
        var newSize = newBounds.Size;

        double relX = oldSize.X.Millimeters > 1e-10
            ? (worldPt.X.Millimeters - oldBounds.Min.X.Millimeters) / oldSize.X.Millimeters
            : 0.5;
        double relY = oldSize.Y.Millimeters > 1e-10
            ? (worldPt.Y.Millimeters - oldBounds.Min.Y.Millimeters) / oldSize.Y.Millimeters
            : 0.5;

        return new Unit2D(
            newBounds.Min.X + Unit.FromMillimeters(relX * newSize.X.Millimeters),
            newBounds.Min.Y + Unit.FromMillimeters(newSize.Y.Millimeters * relY));
    }

    private static UnitBounds RemapWorldBounds(UnitBounds bounds, UnitBounds oldBounds, UnitBounds newBounds)
    {
        return UnitBounds.FromMinMax(
            RemapWorldPoint(bounds.Min, oldBounds, newBounds),
            RemapWorldPoint(bounds.Max, oldBounds, newBounds));
    }

    public override void AssignFrom(ElementGroup other)
    {
        UnsubscribeChildren(_children);
        _children = new(other.Children.Select(child => child.DeepClone()));
        _groupHandleSource.SetChildren(_children);
        SubscribeChildren(_children);

        Transform = other.Transform;

        ChildrenChanged?.Invoke();
    }
}
