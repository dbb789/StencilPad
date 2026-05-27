namespace StencilPad.Spatial;

// Walks IGeometryWalker events and stops when the accumulated distance along
// the path reaches the target. Records the segment index and fraction at that
// point. Feed WalkPolygon for start caps, WalkPolygonReversed for end caps.
public sealed class CapDistanceWalker : IGeometryWalker
{
    private static readonly Unit Tolerance = Unit.FromMillimeters(0.000001);
    private const double Step = 0.1;
    private const double MinStep = 0.0001;

    private readonly Unit _distance;
    private bool _started;
    private Unit2D _startPoint;

    public int SegmentIndex { get; private set; }
    public double Fraction { get; private set; }
    
    public CapDistanceWalker(Unit distance)
    {
        _distance = distance;

        SegmentIndex = 0;
        Fraction = 0.0;
    }

    public void Reset()
    {
        _started = false;
        _startPoint = Unit2D.Zero;
        SegmentIndex = 0;
        Fraction = 0.0;
    }
    
    public bool Begin(int segmentCount, bool closed)
    {
        return true;
    }
    
    public bool Line(int segmentIndex, Unit2D from, Unit2D to)
    {
        CheckStarted(from);

        var dFrom = (from - _startPoint).Magnitude;
        var dTo = (to - _startPoint).Magnitude;

        if ((_distance >= dFrom && _distance <= dTo) ||
            (_distance >= dTo && _distance <= dFrom))
        {
            SegmentIndex = segmentIndex;
            Fraction = Unit.InverseLerp(dFrom, dTo, _distance);
            return false;
        }

        return true;
    }

    public bool Arc(int segmentIndex, Unit2D start, Unit2D mid, Unit2D end)
    {
        CheckStarted(start);

        var dStart = (start - _startPoint).Magnitude;
        var dEnd = (end - _startPoint).Magnitude;

        if ((_distance >= dStart && _distance <= dEnd) ||
            (_distance >= dEnd && _distance <= dStart))
        {
            SegmentIndex = segmentIndex;
            Fraction = Unit.InverseLerp(dStart, dEnd, _distance);
            return false;
        }

        return true;
    }

    public bool Bezier(int segmentIndex, Unit2D from, Unit2D c1, Unit2D c2, Unit2D to)
    {
        CheckStarted(from);

        var bezier = new Bezier2D(from, c1, c2, to);
        
        if (bezier.WalkRadius(_startPoint,
                              0.0,
                              1.0,
                              Step,
                              MinStep,
                              _distance,
                              Tolerance,
                              out double t))
        {
            SegmentIndex = segmentIndex;
            Fraction = t;
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
