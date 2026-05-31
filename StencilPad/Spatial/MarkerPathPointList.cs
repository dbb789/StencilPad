using StencilPad.Spatial;

public class MarkerPathPointList
{
    private static readonly Unit BezierTolerance = Unit.FromMillimeters(0.000001);
    private const double BezierStep = 0.1;
    private const double BezierMinStep = 0.0001;

    private record struct MarkerPoint(UnitTransform Transform, SegmentPoint Point);
    
    public List<UnitTransform> Points => _points.Select(x => x.Transform).ToList();
    public bool Balanced => _balanced;
    
    private readonly List<PolygonSegment> _segments;
    private readonly List<MarkerPoint> _points;

    private Unit _spacing;
    private Unit2D _currentPosition;
    private bool _balanced;

    public MarkerPathPointList()
    {
        _segments = new();
        _points = new();
    }

    public void CalculatePath(Polygon polygon, Unit spacing, Unit offset, bool balance)
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

        var initialSegmentIndex = SegmentIndex(segmentOffset);
        var initialSegment = _segments[initialSegmentIndex];
        
        StartSegment(initialSegmentIndex, initialSegment, startFraction);
        ProcessSegment(initialSegmentIndex, initialSegment, startFraction, 1.0);

        for (int i = 1; i < _segments.Count; ++i)
        {
            var nextSegmentIndex = SegmentIndex(i + segmentOffset);
            var segment = _segments[nextSegmentIndex];

            ProcessSegment(nextSegmentIndex, segment, 0, 1);
        }

        ProcessSegment(initialSegmentIndex, initialSegment, 0, startFraction);

        _balanced = false;
        
        if (balance && polygon.Closed && _points.Count > 2)
        {
            _points.RemoveAt(_points.Count - 1);
            
            var firstMarker = _points[0];
            var lastMarker = _points[^1];
            
            // Calculate a line that bisects the line between the first and last
            // marker with a length of spacing * 2. Any point landing on this
            // line can act as a balancing marker.
            var start = firstMarker.Transform.Position;
            var end = lastMarker.Transform.Position;
            var mid = (end + start) / 2;
            var direction = (end - start);
            var perpendicular = new Unit2D(-direction.Y, direction.X);
            var perpendicularOffset = perpendicular.NormalizedTo(spacing);
            var bisector = new Line(mid + perpendicularOffset,
                                    mid - perpendicularOffset);

            var segmentIndex = lastMarker.Point.Index;

            while (true)
            {
                var segment = _segments[segmentIndex];
                var t = FindIntersection(segmentIndex, segment, bisector);

                if (FindIntersection(segmentIndex, segment, bisector))
                {
                    _balanced = true;
                    break;
                }
                
                if (segmentIndex == firstMarker.Point.Index)
                {
                    break;
                }

                segmentIndex = SegmentIndex(segmentIndex + 1);
            }
        }
    }

    private int SegmentIndex(int index)
    {   
        return ((index % _segments.Count) + _segments.Count) % _segments.Count;
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
        Walk(segmentIndex, t0,
             t => bezier.WalkRadius(_currentPosition, t, t1, BezierStep, BezierMinStep, _spacing, BezierTolerance),
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

    private bool FindIntersection(int segmentIndex, PolygonSegment segment, Line bisector)
    {
        if (segment.IsLine)
        {
            var line = segment.Line;

            if (FindIntersection(segmentIndex, line, bisector))
            {
                return true;
            }
        }
        else if (segment.IsArc)
        {
            var arc = segment.Arc;
            
            var (t0, t1) = arc.Intersection(bisector);

            if (t0 is not null)
            {
                var point = arc.At(t0.Value);
                var direction = arc.Deriv(t0.Value);

                AddPoint(point, direction, new SegmentPoint(segmentIndex, t0.Value));

                return true;
            }

            if (t1 is not null)
            {
                var point = arc.At(t1.Value);
                var direction = arc.Deriv(t1.Value);

                AddPoint(point, direction, new SegmentPoint(segmentIndex, t1.Value));

                return true;
            }

        }
        else if (segment.IsBezier)
        {
            var bezier = segment.Bezier;
            double t = 0;

            while (bezier.Iterate(t, 1, BezierStep, BezierMinStep, BezierTolerance, out double next))
            {
                var line = new Line(bezier.At(t), bezier.At(next));

                if (FindIntersection(segmentIndex, line, bisector))
                {
                    return true;
                }

                t = next;
            }
        }
        else
        {
            throw new InvalidOperationException("Unknown polygon segment type.");
        }

        return false;
    }

    private bool FindIntersection(int segmentIndex, Line line, Line bisector)
    {
        var t = line.Intersection(bisector);
        
        if (t is not null)
        {
            var point = line.At(t.Value);
            var direction = line.Deriv(t.Value);
            
            AddPoint(point, direction, new SegmentPoint(segmentIndex, t.Value));
            
            return true;
        }

        return false;
    }
}
