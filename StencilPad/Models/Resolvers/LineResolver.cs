namespace StencilPad.Spatial;

public class LineResolver : IGeometryResolver
{
    private readonly Line _line;
    
    public LineResolver(Line line)
    {
        _line = line;
    }

    public void Walk(IGeometryWalker walker)
    {
        if (!walker.Begin(1, false))
        {
            return;
        }
        
        walker.Segment(0, PolygonSegment.FromLine(_line));
    }

    public void WalkReverse(IGeometryWalker walker)
    {
        if (!walker.Begin(1, false))
        {
            return;
        }
        
        walker.Segment(0, PolygonSegment.FromLine(_line.Reversed));
    }

    public void WalkEdge(IGeometryWalker walker, int edgeIndex)
    {
        Walk(walker);
    }
}
