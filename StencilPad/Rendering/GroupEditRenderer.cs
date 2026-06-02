using System.Windows.Media;
using StencilPad.Models;

namespace StencilPad.Rendering;

public class GroupEditRenderer : SheetElementEditRenderer
{
    private readonly ElementGroup _elementGroup;
    private readonly List<SheetElementEditRenderer> _childRenderers;

    public GroupEditRenderer(ElementGroup elementGroup)
    {
        _elementGroup = elementGroup;
        
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
    }

    public override void Dispose()
    {
        _elementGroup.ChildrenChanged -= RebuildRenderers;

        foreach (var renderer in _childRenderers.ToList())
        {
            RemoveRenderer(renderer);
        }
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
        _childRenderers.ForEach(renderer => renderer.Render(dc));
    }
}
