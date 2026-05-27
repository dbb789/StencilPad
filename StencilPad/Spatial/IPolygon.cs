namespace StencilPad.Spatial;

public interface IPolygon
{
    IPolygonResolver Resolver { get; }
    
    event Action<IPolygon>? GeometryChanged;

    IKeyedList<Vertex> Vertices { get; }
    IKeyedList<Edge> Edges { get; }
    bool Closed { get; }
}
