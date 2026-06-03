namespace StencilPad.Models.Resolvers;
using StencilPad.Services;

public static class ResolverFactory
{
    public static IStyledGeometryResolver? Create(ISheetElement element,
                                                  IResourceService resourceService)
    {
        if (element is Shape)
        {
            return new ShapeResolver((Shape)element, resourceService);
        }

        return null;
    }
}
