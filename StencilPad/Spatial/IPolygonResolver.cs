namespace StencilPad.Spatial;

public interface IPolygonResolver
{
    void WalkPolygon(IPolygonWalker walker);
    void WalkEdge(IPolygonWalker walker, int edgeIndex);
}
