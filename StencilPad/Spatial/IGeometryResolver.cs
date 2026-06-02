namespace StencilPad.Spatial;

public interface IGeometryResolver
{
    void WalkPolygon(IGeometryWalker walker);
    void WalkPolygonReverse(IGeometryWalker walker);
    void WalkEdge(IGeometryWalker walker, int edgeIndex);
}
