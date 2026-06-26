namespace StencilPad.Spatial;

public class EmptyGeometryResolver :  IGeometryResolver
{
    public static readonly EmptyGeometryResolver Instance = new();
    
    private EmptyGeometryResolver()
    {
        // ...
    }
    
    public void Walk(IGeometryWalker walker)
    {
        walker.Begin(0, false);
    }
    
    public void WalkReverse(IGeometryWalker walker)
    {
        walker.Begin(0, false);
    }
    
    public void WalkEdge(IGeometryWalker walker, int edgeIndex)
    {
        // ...
    }
}
