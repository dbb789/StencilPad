using System.Windows.Media;
using StencilPad.Models.Resolvers;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class ModelRenderer : SheetElementRenderer, IModelWalker, IWalkerRenderer
{
    private readonly IResourceService _resourceService;
    private readonly List<IWalkerRenderer> _renderers;
    private IModelResolver _resolver;
    private Transform _transform;

    public ModelRenderer(IModelResolver resolver, IResourceService resourceService)
    {
        _resolver = resolver;
        _resourceService = resourceService;
        _renderers = new();
        _transform = Transform.Identity;
        _resolver.Attach(this);
    }

    public override void Dispose()
    {
        _resolver.Detach();
        
        foreach (var renderer in _renderers)
        {
            renderer.RendererDirty -= InvokeRendererDirty;
            renderer.Dispose();
        }

        _renderers.Clear();
    }

    public IModelWalker CreateModelWalker(IModelResolver resolver)
    {
        var renderer = new ModelRenderer(resolver, _resourceService);
        
        renderer.RendererDirty += InvokeRendererDirty;

        _renderers.Add(renderer);
        
        return renderer;
    }
    
    public IStyledGeometryWalker CreateStyledGeometryWalker()
    {
        var renderer = new StyledGeometryRenderer(_resourceService);
        
        renderer.RendererDirty += InvokeRendererDirty;

        _renderers.Add(renderer);
        
        return renderer;
    }

    public ITextWalker CreateTextWalker()
    {
        var renderer = new TextRenderer();
        
        renderer.RendererDirty += InvokeRendererDirty;

        _renderers.Add(renderer);
        
        return renderer;
    }

    public void SetTransform(UnitTransform transform)
    {
        _transform = transform.CreateGroupTransform();
        InvokeRendererDirty();
    }
    
    public override void Render(DrawingContext dc)
    {
        dc.PushTransform(_transform);
        
        foreach (var renderer in _renderers)
        {
            renderer.Render(dc);
        }

        dc.Pop();
    }
}
