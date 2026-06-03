namespace StencilPad.Models.Resolvers;
using StencilPad.Services;

public static class ResolverFactory
{
    public static IStyledGeometryResolver? Create(ISheetElement element,
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

        return null;
    }
}
