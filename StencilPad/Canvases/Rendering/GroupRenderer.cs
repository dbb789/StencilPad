using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Rendering;

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

            return bounds ?? UnitBounds.Empty;
        }
    }

    private readonly ElementGroup _elementGroup;
    private readonly List<SheetElementRenderer> _childRenderers;

    public GroupRenderer(ElementGroup elementGroup)
    {
        _elementGroup = elementGroup;
        _childRenderers = [];
        
        foreach (var child in _elementGroup.Children)
        {
            var renderer = SheetElementRendererFactory.Create(child);

            if (renderer is not null)
            {
                AddRenderer(renderer);
            }
        }
    }

    public override void Dispose()
    {
        foreach (var renderer in _childRenderers.ToList())
        {
            RemoveRenderer(renderer);
        }
    }

    private void AddRenderer(SheetElementRenderer renderer)
    {
        _childRenderers.Add(renderer);
        renderer.InvalidateVisual += InvokeInvalidateVisual;
    }

    private void RemoveRenderer(SheetElementRenderer renderer)
    {
        _childRenderers.Remove(renderer);
        renderer.InvalidateVisual -= InvokeInvalidateVisual;
        renderer.Dispose();
    }
    
    public override bool HitTest(Unit2D unit)
    {
        return _childRenderers.Any(renderer => renderer.HitTest(unit));
    }

    public override bool BoundsTest(UnitBounds bounds)
    {
        return _childRenderers.Any(renderer => renderer.BoundsTest(bounds));
    }

    public override void Render(DrawingContext dc)
    {
        _childRenderers.ForEach(renderer => renderer.Render(dc));
    }
}
