using StencilPad.Models;
using StencilPad.Models.Resolvers;
using StencilPad.Services;

namespace StencilPad.Rendering;

public class SheetElementRendererFactory : ISheetElementRendererFactory
{
    private readonly IResourceService _resourceService;
    
    public SheetElementRendererFactory(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }
    
    public SheetElementRenderer? Create(ISheetElement element)
    {
        var resolver = ResolverFactory.Create(element, _resourceService);

        if (resolver is not null)
        {
            return new ResolverRenderer(resolver, _resourceService);
        }
        
        if (element is Ruler ruler)
        {
            return new RulerRenderer(ruler, _resourceService);
        }
        
        if (element is ElementGroup elementGroup)
        {
            return new GroupRenderer(elementGroup, this);
        }
        
        if (element is TextElement textElement)
        {
            return new TextElementRenderer(textElement);
        }
        
        if (element is ImageElement imageElement)
        {
            return new ImageElementRenderer(imageElement);
        }

        return null;
    }
}
