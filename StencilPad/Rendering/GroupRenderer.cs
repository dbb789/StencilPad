using System.Windows.Media;
using StencilPad.Models;

namespace StencilPad.Rendering;

public class GroupRenderer : SheetElementRenderer
{
    private readonly ElementGroup _elementGroup;
    private readonly SheetElementRendererFactory _rendererFactory;
    private readonly List<SheetElementRenderer> _childRenderers;
    private Transform? _transform;

    public GroupRenderer(ElementGroup elementGroup,
                         SheetElementRendererFactory rendererFactory)
    {
        _elementGroup = elementGroup;
        _elementGroup.TransformChanged += TransformChanged;
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
