namespace StencilPad.Spatial;

public class LineResolver : IGeometryResolver
{
    public Line Line;
    
    public LineResolver()
    {
        Line = new Line(Unit2D.Zero, Unit2D.Zero);
    }

    public void Walk(IGeometryWalker walker)
    {
        if (!walker.Begin(1, false))
        {
            return;
        }
        
        walker.Segment(0, PolygonSegment.FromLine(Line));
    }

    public void WalkReverse(IGeometryWalker walker)
    {
        if (!walker.Begin(1, false))
        {
            return;
        }
        
        walker.Segment(0, PolygonSegment.FromLine(Line.Reversed));
    }

    public void WalkEdge(IGeometryWalker walker, int edgeIndex)
    {
        Walk(walker);
    }
}
