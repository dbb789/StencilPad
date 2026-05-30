namespace StencilPad.Spatial;

// Walks IGeometryWalker events and stops when the accumulated distance along
// the path reaches the target. Records the segment index and fraction at that
// point. Feed WalkPolygon for start caps, WalkPolygonReversed for end caps.
public sealed class CapDistanceWalker : IGeometryWalker
{
    private static readonly Unit Tolerance = Unit.FromMillimeters(0.000001);
    private const double Step = 0.1;
    private const double MinStep = 0.0001;

    private Unit _distance;
    private bool _started;
    private Unit2D _startPoint;

    public SegmentPoint? Point { get; private set; }
    
    public CapDistanceWalker()
    {
        Reset(Unit.Zero);
    }

    public void Reset(Unit distance)
    {
        _distance = distance;
        _started = false;
        _startPoint = Unit2D.Zero;
        
        Point = null;
    }
    
    public bool Begin(int segmentCount, bool closed)
    {
        return true;
    }
    
    public bool Line(int segmentIndex, Line line)
    {
        var from = line.Start;
        var to = line.End;
        
        CheckStarted(from);

        var dFrom = (from - _startPoint).Magnitude;
        var dTo = (to - _startPoint).Magnitude;

        if ((_distance >= dFrom && _distance <= dTo) ||
            (_distance >= dTo && _distance <= dFrom))
        {
            Point = new SegmentPoint(segmentIndex,
                                     Unit.InverseLerp(dFrom, dTo, _distance));
            return false;
        }

        return true;
    }

    public bool Arc(int segmentIndex, Arc arc)
    {
        var start = arc.Start;
        var end = arc.End;
        
        CheckStarted(start);

        var dStart = (start - _startPoint).Magnitude;
        var dEnd = (end - _startPoint).Magnitude;

        if ((_distance >= dStart && _distance <= dEnd) ||
            (_distance >= dEnd && _distance <= dStart))
        {
            Point = new SegmentPoint(segmentIndex,
                                     Unit.InverseLerp(dStart, dEnd, _distance));
            return false;
        }

        return true;
    }

    public bool Bezier(int segmentIndex, Bezier2D bezier)
    {
        CheckStarted(bezier.P0);

        if (bezier.WalkRadius(_startPoint,
                              0.0,
                              1.0,
                              Step,
                              MinStep,
                              _distance,
                              Tolerance,
                              out double t))
        {
            Point = new SegmentPoint(segmentIndex, t);
            return false;
        }

        return true;
    }

    private void CheckStarted(Unit2D point)
    {
        if (!_started)
        {
            _started = true;
            _startPoint = point;
        }
    }
}
