namespace StencilPad.Spatial;

public class PolygonSegmentCollector : IGeometryWalker
{
    private List<PolygonSegment> _segments;

    public PolygonSegmentCollector(List<PolygonSegment> segments)
    {
        _segments = segments;
    }

    public bool Begin(int segmentCount, bool closed)
    {
        return true;
    }

    public bool Segment(int segmentIndex, PolygonSegment segment)
    {
        _segments.Add(segment);

        return true;
    }
}
