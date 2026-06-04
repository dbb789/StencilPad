using System.Windows.Media;
using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Overlays;

// This is a special case that's hardwired into ToolOverlay so that individual
// renderers don't have to worry about groups, and so that we can pass
// registered overlays down through the group hierarchy transparently.
public class GroupToolOverlayRenderer : IToolOverlayRenderer
{
    private readonly ElementGroup _group;
    private readonly List<IToolOverlayRendererFactory> _factories;
    private readonly Dictionary<ISheetElement, IToolOverlayRenderer> _renderers;
    private Transform _transform;
    
    public event Action? RendererDirty;
    
    public GroupToolOverlayRenderer(ElementGroup group,
                                    List<IToolOverlayRendererFactory> factories)
    {
        _group = group;
        _factories = new(factories);
        _renderers = new();
        _transform = _group.Transform.CreateGroupTransform();
        
        _group.ChildrenChanged += BuildRenderers;
        _group.TransformChanged += TransformChanged;
        
        BuildRenderers();
    }

    public void Dispose()
    {
        ClearRenderers();
        
        _group.ChildrenChanged -= BuildRenderers;
        _group.TransformChanged -= TransformChanged;
    }
    
    public void Render(DrawingContext dc)
    {
        dc.PushTransform(_transform);

        foreach (var renderer in _renderers.Values)
        {
            renderer.Render(dc);
        }

        dc.Pop();
    }
    
    public void RegisterOverlay(IToolOverlayRendererFactory factory)
    {
        _factories.Add(factory);

        foreach (var element in _group.Children)
        {
            if (!_renderers.ContainsKey(element))
            {
                var renderer = factory.CreateOverlay(element);

                if (renderer is not null)
                {
                    renderer.RendererDirty += InvokeRendererDirty;
                    _renderers.Add(element, renderer);
                }
            }
        }

        foreach (var renderer in _renderers.Values)
        {
            if (renderer is GroupToolOverlayRenderer groupRenderer)
            {
                groupRenderer.RegisterOverlay(factory);
            }
        }
    }
    
    private void TransformChanged(ISheetElement element)
    {
        _transform = _group.Transform.CreateGroupTransform();
        
        InvokeRendererDirty();
    }

    private void BuildRenderers()
    {
        ClearRenderers();

        foreach (var child in _group.Children)
        {
            foreach (var factory in _factories)
            {
                var renderer = factory.CreateOverlay(child);

                if (renderer is not null)
                {
                    renderer.RendererDirty += InvokeRendererDirty;
                    _renderers.Add(child, renderer);
                    break;
                }
            }
        }

        InvokeRendererDirty();
    }

    private void ClearRenderers()
    {
        foreach (var renderer in _renderers.Values)
        {
            renderer.RendererDirty -= InvokeRendererDirty;
            renderer.Dispose();
        }
        
        _renderers.Clear();
    }

    private void InvokeRendererDirty()
    {
        RendererDirty?.Invoke();
    }
}
