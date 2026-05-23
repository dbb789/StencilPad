namespace StencilPad.Spatial;

public interface IPolygon
{
    event Action? GeometryChanged;

    IKeyedList<Vertex> Vertices { get; }
    IKeyedList<Edge> Edges { get; }
    bool Closed { get; }
}
