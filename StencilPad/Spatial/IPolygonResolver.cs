namespace StencilPad.Spatial;

public interface IPolygonResolver
{
    void WalkPolygon(IGeometryWalker walker);
    void WalkPolygonReverse(IGeometryWalker walker);
    void WalkEdge(IGeometryWalker walker, int edgeIndex);
}
