using System.Windows.Media;
using StencilPad.Models.Resolvers;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class ModelRenderer : SheetElementRenderer, IModelWalker
{
    private readonly IResourceService _resourceService;
    private readonly List<StyledGeometryRenderer> _geometryRenderers;
    private IModelResolver _resolver;
    private Transform _transform;

    public ModelRenderer(IModelResolver resolver, IResourceService resourceService)
    {
        _resolver = resolver;
        _resourceService = resourceService;
        _geometryRenderers = new();
        _transform = Transform.Identity;
        _resolver.Attach(this);
    }

    public override void Dispose()
    {
        _resolver.Detach();
        
        foreach (var renderer in _geometryRenderers)
        {
            renderer.RendererDirty -= InvokeRendererDirty;
            renderer.Dispose();
        }

        _geometryRenderers.Clear();
    }

    public IStyledGeometryWalker CreateStyledGeometryWalker()
    {
        var renderer = new StyledGeometryRenderer(_resourceService);
        
        renderer.RendererDirty += InvokeRendererDirty;

        _geometryRenderers.Add(renderer);
        
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
        
        foreach (var renderer in _geometryRenderers)
        {
            renderer.Render(dc);
        }

        dc.Pop();
    }
}
