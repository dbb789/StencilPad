using System.Windows.Media;
using StencilPad.Models;

namespace StencilPad.Rendering;

public class GroupEditRenderer : SheetElementEditRenderer
{
    private readonly ElementGroup _elementGroup;
    private readonly List<SheetElementEditRenderer> _childRenderers;
    private Transform? _transform;

    public GroupEditRenderer(ElementGroup elementGroup)
    {
        _elementGroup = elementGroup;
        _elementGroup.TransformChanged += TransformChanged;
        
        _childRenderers = new(_elementGroup.Children.Count());
        
        foreach (var child in _elementGroup.Children)
        {
            var renderer = SheetElementEditRendererFactory.Create(child);

            if (renderer is not null)
            {
                AddRenderer(renderer);
            }
        }

        _elementGroup.ChildrenChanged += RebuildRenderers;

        TransformChanged(_elementGroup);
    }

    public override void Dispose()
    {
        _elementGroup.ChildrenChanged -= RebuildRenderers;
        _elementGroup.TransformChanged -= TransformChanged;

        foreach (var renderer in _childRenderers.ToList())
        {
            RemoveRenderer(renderer);
        }
    }
    
    private void TransformChanged(ISheetElement element)
    {
        _transform = _elementGroup.Transform.CreateGroupTransform();
        InvokeRendererDirty();
    }

    private void RebuildRenderers()
    {
        foreach (var renderer in _childRenderers.ToList())
        {
            RemoveRenderer(renderer);
        }

        foreach (var child in _elementGroup.Children)
        {
            var renderer = SheetElementEditRendererFactory.Create(child);

            if (renderer is not null)
            {
                AddRenderer(renderer);
            }
        }

        InvokeRendererDirty();
    }

    private void AddRenderer(SheetElementEditRenderer renderer)
    {
        _childRenderers.Add(renderer);
        renderer.RendererDirty += InvokeRendererDirty;
    }

    private void RemoveRenderer(SheetElementEditRenderer renderer)
    {
        _childRenderers.Remove(renderer);
        renderer.RendererDirty -= InvokeRendererDirty;
        renderer.Dispose();
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
