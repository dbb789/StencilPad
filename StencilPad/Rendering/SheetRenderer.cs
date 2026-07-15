using System.Windows.Media;
using Microsoft.Extensions.Logging;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Resolvers;

namespace StencilPad.Rendering;

public class SheetRenderer : IDisposable
{
    public class Factory(ILogger<SheetRenderer> Logger,
                         ISettings Settings,
                         IResourceSet ResourceSet)
    {
        public SheetRenderer Create(SheetResolver resolver)
        {
            return new(Logger, resolver, Settings, ResourceSet);
        }
    }
    
    private readonly ILogger<SheetRenderer> _logger;
    private readonly SheetResolver _resolver;
    private readonly ISettings _settings;
    private readonly IResourceSet _resourceSet;
    private readonly OrderedDictionary<ISheetElementResolver, ModelRenderer> _renderers;
    
    public event Action? RendererDirty;

    private SheetRenderer(ILogger<SheetRenderer> logger,
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
        
        _resolver.ElementsChanged += OnElementsChanged;
    }

    public void Dispose()
    {
        foreach (var modelResolver in _resolver.Elements)
        {
            OnElementRemoved(modelResolver);
        }

        _resolver.ElementsChanged -= OnElementsChanged;
    }

    public void Render(DrawingContext dc)
    {
        foreach (var (_, renderer) in _renderers)
        {
            renderer.Render(dc);
        }
    }

    private void OnElementsChanged(ObservableListChangedArgs<ISheetElementResolver> e)
    {
        switch (e.Action)
        {
        case ObservableListChangedAction.Add:
            OnElementAdded(e.Item, e.NewIndex);
            break;
            
        case ObservableListChangedAction.Remove:
            OnElementRemoved(e.Item);
            break;

        case ObservableListChangedAction.Move:
            OnElementMoved(e.OldIndex, e.NewIndex);
            break;
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

    private void OnElementMoved(int oldIndex, int newIndex)
    {
        var kvp = _renderers.GetAt(oldIndex);
        
        _renderers.RemoveAt(oldIndex);
        _renderers.Insert(newIndex, kvp.Key, kvp.Value);

        InvokeRendererDirty();
    }
    
    private void InvokeRendererDirty()
    {
        RendererDirty?.Invoke();
    }
}
