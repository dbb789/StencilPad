namespace StencilPad.Spatial;

public interface IPolygon
{
    public AssignableList<Vertex> Vertices { get; }
    public AssignableList<Edge> Edges { get; }
    public bool Closed { get; }
}
