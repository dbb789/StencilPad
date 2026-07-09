using System.Windows.Media;
using Microsoft.Extensions.Logging;
using StencilPad.Common;
using StencilPad.Models.Resolvers;

namespace StencilPad.Rendering;

public class SheetRenderer : IDisposable
{
    private readonly ILogger<SheetRenderer> _logger;
    private readonly SheetResolver _resolver;
    private readonly ISettings _settings;
    private readonly IResourceSet _resourceSet;
    private readonly OrderedDictionary<ISheetElementResolver, ModelRenderer> _renderers;
    
    public event Action? RendererDirty;

    public SheetRenderer(ILogger<SheetRenderer> logger,
                         SheetResolver resolver,
                         ISettings settings,
                         IResourceSet resourceSet)
    {
        _logger = logger;
        _resolver = resolver;
        _settings = settings;
        _resourceSet = resourceSet;
        _renderers = new();

        int index = 0;
        
        foreach (var modelResolver in _resolver.Elements)
        {
            OnElementAdded(modelResolver, index++);
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

    private void OnElementAdded(ISheetElementResolver resolver, int index)
    {
        var renderer = new ModelRenderer(_resourceSet);

        renderer.RendererDirty += InvokeRendererDirty;
        resolver.Attach(renderer);
        _renderers.Insert(index, resolver, renderer);
        
        InvokeRendererDirty();
    }

    private void OnElementRemoved(ISheetElementResolver resolver)
    {
        if (!_renderers.TryGetValue(resolver, out var renderer))
        {
            _logger.LogError("Could not find renderer for resolver {ResolverType}.", resolver.GetType().Name);
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
