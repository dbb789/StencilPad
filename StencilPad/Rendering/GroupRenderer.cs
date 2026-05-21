using System.ComponentModel;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class GroupRenderer : SheetElementRenderer
{
    public override ElementGroup Element => _elementGroup;
    public override UnitBounds SelectionBounds
    {
        get
        {
            UnitBounds? bounds = null;
            
            foreach (var child in _childRenderers)
            {
                bounds = UnitBounds.Union(bounds, child.SelectionBounds);
            }

            return (bounds ?? UnitBounds.Empty).ApplyTransform(_elementGroup.Transform);
        }
    }

    private readonly ElementGroup _elementGroup;
    private readonly SheetElementRendererFactory _rendererFactory;
    private readonly List<SheetElementRenderer> _childRenderers;
    private Transform? _transform;

    public GroupRenderer(ElementGroup elementGroup,
                         SheetElementRendererFactory rendererFactory)
    {
        _elementGroup = elementGroup;
        _elementGroup.PropertyChanged += PropertyChanged;
        _rendererFactory = rendererFactory;
        
        _childRenderers = new(_elementGroup.Children.Count());
        
        foreach (var child in _elementGroup.Children)
        {
            var renderer = _rendererFactory.Create(child);

            if (renderer is not null)
            {
                AddRenderer(renderer);
            }
        }

        _elementGroup.ChildrenChanged += RebuildRenderers;

        UpdateProperties();
    }

    public override void Dispose()
    {
        _elementGroup.ChildrenChanged -= RebuildRenderers;
        _elementGroup.PropertyChanged -= PropertyChanged;

        foreach (var renderer in _childRenderers.ToList())
        {
            RemoveRenderer(renderer);
        }
    }
    
    private void PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ElementGroup.Transform))
        {
            UpdateProperties();
        }
        InvokeRendererDirty();
    }
    
    private void UpdateProperties()
    {
        var group = new TransformGroup();
        if (_elementGroup.Transform.Angle != 0m)
        {
            group.Children.Add(new RotateTransform((double)_elementGroup.Transform.Angle));
        }
        group.Children.Add(new TranslateTransform(_elementGroup.Transform.Position.X.Millimeters,
                                                  _elementGroup.Transform.Position.Y.Millimeters));
        group.Freeze();
        _transform = group;
    }

    private void RebuildRenderers()
    {
        foreach (var renderer in _childRenderers.ToList())
        {
            RemoveRenderer(renderer);
        }

        foreach (var child in _elementGroup.Children)
        {
            var renderer = _rendererFactory.Create(child);

            if (renderer is not null)
            {
                AddRenderer(renderer);
            }
        }

        InvokeRendererDirty();
    }

    private void AddRenderer(SheetElementRenderer renderer)
    {
        _childRenderers.Add(renderer);
        renderer.RendererDirty += InvokeRendererDirty;
    }

    private void RemoveRenderer(SheetElementRenderer renderer)
    {
        _childRenderers.Remove(renderer);
        renderer.RendererDirty -= InvokeRendererDirty;
        renderer.Dispose();
    }
    
    public override bool HitTest(Unit2D unit)
    {
        return _childRenderers.Any(renderer => renderer.HitTest(_elementGroup.Transform.InverseApply(unit)));
    }

    public override bool BoundsTest(UnitBounds bounds)
    {
        // For groups, we transform the selection bounds into the local space.
        // Similar to ShapeRenderer, we could create a local geometry but for groups
        // it's easier to just pass the transformed bounds to children.
        // Wait, BoundsTest on children expects global bounds (relative to group origin).
        // So we just need to transform the incoming global bounds into the group's local space.
        
        // This is complex because a transformed UnitBounds is no longer a UnitBounds.
        // However, we can use the same logic as ShapeRenderer if we want to be precise.
        // For now, let's just use the inverse transform on the bounds.
        
        var localNW = _elementGroup.Transform.InverseApply(bounds.NW);
        var localNE = _elementGroup.Transform.InverseApply(bounds.NE);
        var localSW = _elementGroup.Transform.InverseApply(bounds.SW);
        var localSE = _elementGroup.Transform.InverseApply(bounds.SE);

        var localBounds = UnitBounds.FromMinMax(
            new Unit2D(Unit.Min(Unit.Min(localNW.X, localNE.X), Unit.Min(localSW.X, localSE.X)),
                       Unit.Min(Unit.Min(localNW.Y, localNE.Y), Unit.Min(localSW.Y, localSE.Y))),
            new Unit2D(Unit.Max(Unit.Max(localNW.X, localNE.X), Unit.Max(localSW.X, localSE.X)),
                       Unit.Max(Unit.Max(localNW.Y, localNE.Y), Unit.Max(localSW.Y, localSE.Y))));

        return _childRenderers.Any(renderer => renderer.BoundsTest(localBounds));
    }

    public override void Render(DrawingContext dc)
    {
        if (_transform is null)
        {
            return;
        }

        dc.PushTransform(_transform);
        _childRenderers.ForEach(renderer => renderer.Render(dc));
        dc.Pop();
    }
}
