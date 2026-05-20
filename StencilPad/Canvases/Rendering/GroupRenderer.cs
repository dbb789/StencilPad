using System.ComponentModel;
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

            return (bounds ?? UnitBounds.Empty) + _elementGroup.Position;
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
        UpdateProperties();
        InvokeInvalidateVisual();
    }
    
    private void UpdateProperties()
    {
        _transform = new TranslateTransform(_elementGroup.Position.X.Millimeters,
                                            _elementGroup.Position.Y.Millimeters);
        _transform.Freeze();
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

        InvokeInvalidateVisual();
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
        return _childRenderers.Any(renderer => renderer.HitTest(unit - _elementGroup.Position));
    }

    public override bool BoundsTest(UnitBounds bounds)
    {
        return _childRenderers.Any(renderer => renderer.BoundsTest(bounds - _elementGroup.Position));
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
