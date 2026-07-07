using System.Diagnostics;
using System.Windows.Media;
using StencilPad.Common;
using StencilPad.Models.Resolvers;

namespace StencilPad.Rendering;

public class SheetRenderer : IDisposable
{
    private readonly SheetResolver _resolver;
    private readonly ISettings _settings;
    private readonly IResourceSet _resourceSet;
    private readonly OrderedDictionary<ISheetElementResolver, ModelRenderer> _renderers;
    
    public event Action? RendererDirty;

    public SheetRenderer(SheetResolver resolver,
                         ISettings settings,
                         IResourceSet resourceSet)
    {
        _resolver = resolver;
        _settings = settings;
        _resourceSet = resourceSet;
        _renderers = new();

        foreach (var modelResolver in _resolver.Elements)
        {
            OnElementAdded(modelResolver);
        }
        
        _resolver.ElementAdded += OnElementAdded;
        _resolver.ElementRemoved += OnElementRemoved;
    }

    public void Dispose()
    {
        foreach (var modelResolver in _resolver.Elements)
        {
            OnElementRemoved(modelResolver);
        }
        
        _resolver.ElementAdded -= OnElementAdded;
        _resolver.ElementRemoved -= OnElementRemoved;
    }

    public void Render(DrawingContext dc)
    {
        foreach (var (_, renderer) in _renderers)
        {
            renderer.Render(dc);
        }
    }

    private void OnElementAdded(ISheetElementResolver resolver)
    {
        var renderer = new ModelRenderer(_resourceSet);

        renderer.RendererDirty += InvokeRendererDirty;
        resolver.Attach(renderer);
        _renderers.Add(resolver, renderer);
        
        InvokeRendererDirty();
    }

    private void OnElementRemoved(ISheetElementResolver resolver)
    {
        if (!_renderers.TryGetValue(resolver, out var renderer))
        {
            Debug.WriteLine("Could not find renderer for resolver");
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
