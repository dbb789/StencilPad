using StencilPad.Spatial;

public class MarkerPathPointList
{
    private record struct MarkerPoint(UnitTransform Transform, SegmentPoint Point);
    
    public List<UnitTransform> Points => _points.Select(x => x.Transform).ToList();

    private readonly List<PolygonSegment> _segments;
    private readonly List<MarkerPoint> _points;

    private Unit _spacing;
    private Unit2D _currentPosition;
    
    public MarkerPathPointList()
    {
        _segments = new();
        _points = new();
    }

    public void CalculatePath(Polygon polygon, Unit spacing, Unit offset)
    {
        var distanceWalker = new CapDistanceWalker();

        distanceWalker.Reset(offset);

        polygon.Resolver.WalkPolygon(distanceWalker);

        var startPoint = distanceWalker.Point;

        _segments.Clear();
        _points.Clear();
        _spacing = spacing;

        var collectWalker = new PolygonSegmentCollector(_segments);
        
        polygon.Resolver.WalkPolygon(collectWalker);

        int segmentOffset = 0;
        double startFraction = 0;
        
        if (startPoint is not null)
        {
            segmentOffset = startPoint.Value.Index;
            startFraction = startPoint.Value.Fraction;
        }

        var initialSegmentIndex = SegmentIndex(0, segmentOffset);
        var initialSegment = _segments[initialSegmentIndex];
        
        StartSegment(initialSegmentIndex, initialSegment, startFraction);
        ProcessSegment(initialSegmentIndex, initialSegment, startFraction, 1.0);

        for (int i = 1; i < _segments.Count; ++i)
        {
            var nextSegmentIndex = SegmentIndex(i, segmentOffset);
            var segment = _segments[nextSegmentIndex];

            ProcessSegment(nextSegmentIndex, segment, 0, 1);
        }

        ProcessSegment(initialSegmentIndex, initialSegment, 0, startFraction);
    }

    private int SegmentIndex(int index, int offset)
    {   
        return (((index + offset) % _segments.Count) + _segments.Count) % _segments.Count;
    }
    
    private void StartSegment(int segmentIndex, PolygonSegment segment, double t0)
    {
        if (segment.IsLine)
        {
            AddPoint(segment.Line.At(t0), segment.Line.Deriv(t0), new SegmentPoint(segmentIndex, t0));
        }
        else if (segment.IsArc)
        {
            AddPoint(segment.Arc.At(t0), segment.Arc.Deriv(t0), new SegmentPoint(segmentIndex, t0));
        }
        else if (segment.IsBezier)
        {
            AddPoint(segment.Bezier.At(t0), segment.Bezier.Deriv(t0), new SegmentPoint(segmentIndex, t0));
        }
        else
        {
            throw new InvalidOperationException("Unknown polygon segment type.");
        }
    }

    private void ProcessSegment(int segmentIndex, PolygonSegment segment, double t0, double t1)
    {
        if (segment.IsLine)
        {
            WalkLine(segmentIndex, segment.Line, t0, t1);
        }
        else if (segment.IsArc)
        {
            WalkArc(segmentIndex, segment.Arc, t0, t1);
        }
        else if (segment.IsBezier)
        {
            WalkBezier(segmentIndex, segment.Bezier, t0, t1);
        }
        else
        {
            throw new InvalidOperationException("Unknown polygon segment type.");
        }
    }

    private void WalkLine(int segmentIndex, Line line, double t0, double t1)
    {
        Walk(segmentIndex, t0,
             t => line.FromRadius(_currentPosition, _spacing, t, t1),
             t => (line.At(t), line.Deriv(t)));
    }
    
    private void WalkArc(int segmentIndex, Arc arc, double t0, double t1)
    {
        Walk(segmentIndex, t0,
             t => arc.FromRadius(_currentPosition, _spacing, t, t1),
             t => (arc.At(t), arc.Deriv(t)));
    }
    
    private void WalkBezier(int segmentIndex, Bezier2D bezier, double t0, double t1)
    {
        Unit tolerance = Unit.FromMillimeters(0.000001);
        double step = 0.1;
        double minStep = 0.0001;
        
        Walk(segmentIndex, t0,
             t => bezier.WalkRadius(_currentPosition, t, t1, step, minStep, _spacing, tolerance),
             t => (bezier.At(t), bezier.Deriv(t)));
    }
    
    private double Walk(int segmentIndex,
                        double initialT,
                        Func<double, double?> fromRadius,
                        Func<double, (Unit2D, Unit2D)> at)
    {
        double lastT = initialT;

        while (true)
        {
            var nextT = fromRadius(lastT);
            
            if (nextT is null)
            {
                break;
            }
            
            var (point, deriv) = at(nextT.Value);
            
            AddPoint(point, deriv, new SegmentPoint(segmentIndex, nextT.Value));
            
            lastT = nextT.Value;
        }

        return lastT;
    }

    private void AddPoint(Unit2D position, Unit2D direction, SegmentPoint point)
    {
        var angle = Math.Atan2(direction.Y.Millimeters, direction.X.Millimeters);
        var angleDegrees = (decimal)(angle * MathUtil.Rad2Deg) + 90;
        
        _points.Add(new MarkerPoint(new UnitTransform(position, angleDegrees), point));
        _currentPosition = position;
    }
}
