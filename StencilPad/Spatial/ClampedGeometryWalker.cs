namespace StencilPad.Spatial;

// Wraps an IGeometryWalker and only forwards segments within the range
// [startSegment, startFraction] .. [endSegment, endFraction], splitting
// the boundary segments at the given fractions.
public class ClampedGeometryWalker : IGeometryWalker
{
    private IGeometryWalker _inner;
    private SegmentPoint _startPoint;
    private SegmentPoint _endPoint;

    public ClampedGeometryWalker(IGeometryWalker inner)
    {
        _inner = inner;
        _startPoint = new SegmentPoint(0, 0.0);
        _endPoint = new SegmentPoint(int.MaxValue, 1.0);
    }

    public void SetStartEnd(SegmentPoint? startPoint, SegmentPoint? endPoint)
    {
        _startPoint = startPoint ?? new SegmentPoint(0, 0.0);
        _endPoint = endPoint ?? new SegmentPoint(int.MaxValue, 1.0);
    }
    
    public bool Begin(int segmentCount, bool closed)
    {
        _endPoint = _endPoint with { Index = Math.Min(_endPoint.Index, segmentCount - 1) };

        var clampedSegmentCount = (_endPoint.Index - _startPoint.Index) + 1;
        
        return _inner.Begin(clampedSegmentCount, closed);
    }

    public bool Segment(int segmentIndex, PolygonSegment segment)
    {
        if (segmentIndex < _startPoint.Index || segmentIndex > _endPoint.Index)
        {
            return segmentIndex <= _endPoint.Index;
        }

        var start = (segmentIndex == _startPoint.Index) ? _startPoint.Fraction : 0.0;
        var end = (segmentIndex == _endPoint.Index) ? _endPoint.Fraction   : 1.0;

        return _inner.Segment(segmentIndex - _startPoint.Index, segment.Subsegment(start, end));
    }
}
