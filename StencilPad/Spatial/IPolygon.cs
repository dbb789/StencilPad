using StencilPad.Collections;

namespace StencilPad.Spatial;

public interface IPolygon
{
    IGeometryResolver Resolver { get; }
    
    event Action<IPolygon>? GeometryChanged;

    IKeyedList<Vertex> Vertices { get; }
    IKeyedList<Edge> Edges { get; }
    bool Closed { get; }
}
