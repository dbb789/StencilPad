using StencilPad.Models;
using StencilPad.Services;

namespace StencilPad.Canvases.Rendering;

public class SheetElementRendererFactory : ISheetElementRendererFactory
{
    private readonly IResourceService _resourceService;
    
    public SheetElementRendererFactory(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }
    
    public SheetElementRenderer? Create(ISheetElement element)
    {
        SheetElementRenderer? renderer = null;
        
        if (element is Shape shape)
        {
            renderer = new ShapeRenderer(shape);
        }
        else if (element is MarkerPath markerPath)
        {
            renderer = new MarkerPathRenderer(markerPath);
        }
        else if (element is Ruler ruler)
        {
            renderer = new RulerRenderer(ruler);
        }
        else if (element is ElementGroup elementGroup)
        {
            renderer = new GroupRenderer(elementGroup, this);
        }
        else if (element is TextElement textElement)
        {
            renderer = new TextElementRenderer(textElement);
        }
        else if (element is ImageElement imageElement)
        {
            renderer = new ImageElementRenderer(imageElement);
        }

        return renderer;
    }
}
