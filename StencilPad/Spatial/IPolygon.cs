namespace StencilPad.Spatial;

public interface IPolygon
{
    public IKeyedList<Vertex> Vertices { get; }
    public IKeyedList<Edge> Edges { get; }
    public bool Closed { get; }
}
