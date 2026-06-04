namespace StencilPad.Models.Resolvers;

public static class ResolverFactory
{
    public static IModelResolver? Create(ISheetElement element,
                                         IResourceSet resourceSet)
    {
        if (element is Shape shape)
        {
            return new ShapeResolver(shape, resourceSet);
        }

        if (element is MarkerPath markerPath)
        {
            return new MarkerPathResolver(markerPath, resourceSet);
        }
        
        if (element is Ruler ruler)
        {
            return new RulerResolver(ruler, resourceSet);
        }
        
        if (element is TextElement textElement)
        {
            return new TextElementResolver(textElement);
        }
        
        if (element is ImageElement imageElement)
        {
            return new ImageElementResolver(imageElement);
        }

        if (element is ElementGroup elementGroup)
        {
            return new GroupResolver(elementGroup, resourceSet);
        }

        return null;
    }
}
