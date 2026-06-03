using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public readonly record struct GeometrySet
{
    public readonly IGeometryResolver Resolver;
    public readonly IEnumerable<(GeometryResource, UnitTransform)> Overlays;

    public GeometrySet(IGeometryResolver resolver,
                       IEnumerable<(GeometryResource, UnitTransform)> overlays)
    {
        Resolver = resolver;
        Overlays = overlays;
    }

    public GeometrySet(IGeometryResolver resolver)
    {
        Resolver = resolver;
        Overlays = Enumerable.Empty<(GeometryResource, UnitTransform)>();
    }
}
