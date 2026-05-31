using StencilPad.Spatial;

public class MarkerPathPointList
{
    public List<UnitTransform> Points => _points;

    private readonly List<PolygonSegment> _segments;
    private readonly List<UnitTransform> _points;

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

        System.Diagnostics.Debug.WriteLine($"Segments: {_segments.Count}");

        int segmentOffset = 0;
        
        StartSegment(_segments[SegmentIndex(0, segmentOffset)]);
        
        for (int i = 0; i < _segments.Count; ++i)
        {
            var segment = _segments[SegmentIndex(i, segmentOffset)];

            ProcessSegment(segment);
        }
    }

    private int SegmentIndex(int index, int offset)
    {
        return (index + offset + _segments.Count) % _segments.Count;
    }
    
    private void StartSegment(PolygonSegment segment)
    {
        if (segment.IsLine)
        {
            AddPoint(segment.Line.At(0), segment.Line.Deriv(0));
        }
        else if (segment.IsArc)
        {
            AddPoint(segment.Arc.At(0), segment.Arc.Deriv(0));
        }
        else if (segment.IsBezier)
        {
            AddPoint(segment.Bezier.At(0), segment.Bezier.Deriv(0));
        }
        else
        {
            throw new InvalidOperationException("Unknown polygon segment type.");
        }
    }

    private void ProcessSegment(PolygonSegment segment)
    {
        if (segment.IsLine)
        {
            WalkLine(segment.Line);
        }
        else if (segment.IsArc)
        {
            WalkArc(segment.Arc);
        }
        else if (segment.IsBezier)
        {
            WalkBezier(segment.Bezier);
        }
        else
        {
            throw new InvalidOperationException("Unknown polygon segment type.");
        }
    }

    private void WalkLine(Line line)
    {
        Walk(0,
             t => line.FromRadius(_currentPosition, _spacing, t, 1.0),
             t => (line.At(t), line.Deriv(t)));
    }
    
    private void WalkArc(Arc arc)
    {
        Walk(0,
             t => arc.FromRadius(_currentPosition, _spacing, t, 1.0),
             t => (arc.At(t), arc.Deriv(t)));
    }
    
    private void WalkBezier(Bezier2D bezier)
    {
        Unit tolerance = Unit.FromMillimeters(0.000001);
        double step = 0.1;
        double minStep = 0.0001;
        
        Walk(0,
             t => bezier.WalkRadius(_currentPosition, t, 1.0, step, minStep, _spacing, tolerance),
             t => (bezier.At(t), bezier.Deriv(t)));
    }
    
    private double Walk(double initialT,
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
            
            AddPoint(point, deriv);
            
            lastT = nextT.Value;
        }

        return lastT;
    }

    private void AddPoint(Unit2D position, Unit2D direction)
    {
        var angle = Math.Atan2(direction.Y.Millimeters, direction.X.Millimeters);

        _points.Add(new UnitTransform(position, (decimal)(angle * MathUtil.Rad2Deg) + 90));
        _currentPosition = position;
    }
}
