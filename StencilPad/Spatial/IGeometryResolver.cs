namespace StencilPad.Spatial;

public interface IGeometryResolver
{
    void Walk(IGeometryWalker walker);
    void WalkReverse(IGeometryWalker walker);
    void WalkEdge(IGeometryWalker walker, int edgeIndex);
}
