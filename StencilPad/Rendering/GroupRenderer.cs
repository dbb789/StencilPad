using System.Windows.Media;
using StencilPad.Models;

namespace StencilPad.Rendering;

public class GroupRenderer : SheetElementRenderer
{
    public override ElementGroup Element => _elementGroup;

    private readonly ElementGroup _elementGroup;
    private readonly SheetElementRendererFactory _rendererFactory;
    private readonly List<SheetElementRenderer> _childRenderers;

    public GroupRenderer(ElementGroup elementGroup,
                         SheetElementRendererFactory rendererFactory)
    {
        _elementGroup = elementGroup;
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
        _childRenderers.ForEach(renderer => renderer.Render(dc));
    }
}
