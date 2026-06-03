using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public readonly record struct GeometrySet
{
    public readonly IGeometryResolver Resolver;
    public readonly SegmentPoint? StartPoint;
    public readonly SegmentPoint? EndPoint;
    public readonly IEnumerable<(GeometryResource, UnitTransform)> Overlays;

    public GeometrySet(IGeometryResolver resolver,
                       SegmentPoint? startPoint,
                       SegmentPoint? endPoint,
                       IEnumerable<(GeometryResource, UnitTransform)> overlays)
    {
        Resolver = resolver;
        StartPoint = startPoint;
        EndPoint = endPoint;
        Overlays = overlays;
    }
    
    public GeometrySet(IGeometryResolver resolver,
                       IEnumerable<(GeometryResource, UnitTransform)> overlays)
    {
        Resolver = resolver;
        StartPoint = null;
        EndPoint = null;
        Overlays = overlays;
    }

    public GeometrySet(IGeometryResolver resolver)
    {
        Resolver = resolver;
        StartPoint = null;
        EndPoint = null;
        Overlays = Enumerable.Empty<(GeometryResource, UnitTransform)>();
    }
}
