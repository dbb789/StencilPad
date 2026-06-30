using System.Windows.Media;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Resolvers;

namespace StencilPad.Rendering;

public class SheetRenderer : IDisposable
{
    private readonly SheetResolver _resolver;
    private readonly ISettings _settings;
    private readonly IResourceSet _resourceSet;
    private OrderedDictionary<IModelResolver, ModelRenderer> _renderers;
    
    public event Action? RendererDirty;

    public SheetRenderer(SheetResolver resolver,
                         ISettings settings,
                         IResourceSet resourceSet)
    {
        _resolver = resolver;
        _settings = settings;
        _resourceSet = resourceSet;
        _renderers = new();

        _resolver.ResolverAdded += OnResolverAdded;
        _resolver.ResolverRemoved += OnResolverRemoved;
    }

    public void Dispose()
    {
        _resolver.ResolverAdded -= OnResolverAdded;
        _resolver.ResolverRemoved -= OnResolverRemoved;
    }

    public void Render(DrawingContext dc)
    {
        foreach (var (_, renderer) in _renderers)
        {
            renderer.Render(dc);
        }
    }

    private void OnResolverAdded(ISheetElement element, IModelResolver resolver)
    {
        var renderer = new ModelRenderer(_resourceSet);

        renderer.RendererDirty += InvokeRendererDirty;
        resolver.Attach(renderer);
        _renderers.Add(resolver, renderer);
        
        InvokeRendererDirty();
    }

    private void OnResolverRemoved(ISheetElement element, IModelResolver resolver)
    {
        if (!_renderers.TryGetValue(resolver, out var renderer))
        {
            return;
        }

        renderer.RendererDirty -= InvokeRendererDirty;
        renderer.Dispose();
        _renderers.Remove(resolver);
        
        InvokeRendererDirty();
    }
    
    private void InvokeRendererDirty()
    {
        RendererDirty?.Invoke();
    }
}
