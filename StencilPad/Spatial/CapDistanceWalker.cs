namespace StencilPad.Spatial;

// Walks IGeometryWalker events and stops when the accumulated distance along
// the path reaches the target. Records the segment index and fraction at that
// point. Feed WalkPolygon for start caps, WalkPolygonReversed for end caps.
public class CapDistanceWalker : IGeometryWalker
{
    private Unit _distance;

    public SegmentPoint? Point { get; private set; }
    
    public CapDistanceWalker()
    {
        Reset(Unit.Zero);
    }

    public void Reset(Unit distance)
    {
        _distance = distance;
        Point = null;
    }
    
    public bool Begin(int segmentCount, bool closed)
    {
        return true;
    }
    
    public bool Segment(int segmentIndex, PolygonSegment segment)
    {
        if (segment.IsLine)
        {
            return WalkLine(segmentIndex, segment.Line);
        }

        if (segment.IsArc)
        {
            return WalkArc(segmentIndex, segment.Arc);
        }

        if (segment.IsBezier)
        {
            return WalkBezier(segmentIndex, segment.Bezier);
        }
        
        throw new InvalidOperationException("Unknown polygon segment type.");
    }

    private bool WalkLine(int segmentIndex, Line line)
    {
        var lineLength = line.Length;
        
        if (_distance <= lineLength)
        {
            Point = new SegmentPoint(segmentIndex, _distance / lineLength);
            return false;
        }

        _distance -= line.Length;
        
        return true;
    }

    private bool WalkArc(int segmentIndex, Arc arc)
    {
        var arcLength = arc.Length;
        
        if (_distance <= arc.Length)
        {
            Point = new SegmentPoint(segmentIndex, _distance / arcLength);
            return false;
        }
        
        _distance -= arc.Length;

        return true;
    }

    private bool WalkBezier(int segmentIndex, Bezier2D bezier)
    {
        var (t, walkLength) = bezier.Walk(0.0, 1.0, _distance, Bezier2D.IterateFine);

        if (t < 1.0)
        {
            Point = new SegmentPoint(segmentIndex, t);
            return false;
        }

        _distance -= walkLength;
        
        return true;
    }
}
