using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Models.Resolvers;
using StencilPad.Spatial;
using System.Windows.Media;

namespace StencilPad.Rendering;

public class SheetElementRenderer
{
    public bool HasContent => _modelRenderer is not null;
    
    private IModelResolver? _resolver;
    private ModelRenderer? _modelRenderer;
    
    public event Action? RendererDirty;

    public SheetElementRenderer(ISheetElement element,
                                ISettings settings,
                                IResourceSet resourceSet)
    {
        _resolver = ResolverFactory.Create(element, settings, resourceSet);

        if (_resolver is not null)
        {
            _modelRenderer = new ModelRenderer(resourceSet);
            _modelRenderer.RendererDirty += InvokeRendererDirty;
            
            _resolver.Attach(_modelRenderer);
        }
    }

    public void Dispose()
    {
        if (_resolver is not null)
        {
            _resolver.Dispose();
            _resolver = null;
        }

        if (_modelRenderer is not null)
        {
            _modelRenderer.RendererDirty -= InvokeRendererDirty;
            _modelRenderer.Dispose();
            _modelRenderer = null;
        }
    }

    public void Render(DrawingContext dc)
    {
        _modelRenderer?.Render(dc);
    }

    private void InvokeRendererDirty()
    {
        RendererDirty?.Invoke();
    }
}
