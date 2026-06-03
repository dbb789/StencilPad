namespace StencilPad.Models.Resolvers;
using StencilPad.Services;

public static class ResolverFactory
{
    public static IModelResolver? Create(ISheetElement element,
                                         IResourceService resourceService)
    {
        if (element is Shape shape)
        {
            return new ShapeResolver(shape, resourceService);
        }

        if (element is MarkerPath markerPath)
        {
            return new MarkerPathResolver(markerPath, resourceService);
        }
        
        if (element is Ruler ruler)
        {
            return new RulerResolver(ruler, resourceService);
        }
        
        if (element is TextElement textElement)
        {
            return new TextElementResolver(textElement);
        }

        return null;
    }
}
