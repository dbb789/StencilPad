using StencilPad.Models;
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
        if (element is Shape shape)
        {
            return new ShapeRenderer(shape, _resourceService);
        }
        
        if (element is MarkerPath markerPath)
        {
            return new MarkerPathRenderer(markerPath, _resourceService);
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
