using StencilPad.Spatial;

public class MarkerPathPointList
{
    public readonly record struct Point(Unit2D Position);

    public class MarkerPathWalker : IGeometryWalker
    {
        public IEnumerable<Point> Points => _points;

        private readonly Unit _spacing;
        private readonly Unit _offset;
        
        private bool _started;
        private Unit2D _currentPosition;
        private List<Point> _points;

        public MarkerPathWalker(Unit spacing, Unit offset)
        {
            _spacing = spacing;
            _offset = offset;
            _points = new();
        }
        
        public bool Begin(int segmentCount, bool closed)
        {
            _points.Clear();
            
            return true;
        }
        
        private bool WalkLine(Line line)
        {
            StartSegment(line.Start);
            return WalkLineOrArc(t => line.FromRadius(_currentPosition, _spacing, t, 1.0),
                                 t => line.At(t));
        }
        
        private bool WalkArc(Arc arc)
        {
            StartSegment(arc.Start);
            return WalkLineOrArc(t => arc.FromRadius(_currentPosition, _spacing, t, 1.0),
                                 t => arc.At(t));
        }

        private bool WalkLineOrArc(Func<double, double?> fromRadius, Func<double, Unit2D> at)
        {
            double lastT = -1;

            while (true)
            {
                var nextT = fromRadius(lastT);

                if (nextT is null)
                {
                    break;
                }

                var point = at(nextT.Value);

                _points.Add(new Point(point));

                _currentPosition = point;
                lastT = nextT.Value;
            }

            return true;
        }

        private bool WalkBezier(Bezier2D bezier)
        {
            Unit tolerance = Unit.FromMillimeters(0.000001);
            double step = 0.1;
            double minStep = 0.0001;

            StartSegment(bezier.P0);
            return WalkLineOrArc(t => bezier.WalkRadius(_currentPosition, t, 1.0, step, minStep, _spacing, tolerance),
                                 t => bezier.At(t));
        }

        public bool Segment(int segmentIndex, PolygonSegment segment)
        {
            if (segment.IsLine)
            {
                return WalkLine(segment.Line);
            }

            if (segment.IsArc)
            {
                return WalkArc(segment.Arc);
            }

            if (segment.IsBezier)
            {
                return WalkBezier(segment.Bezier);
            }

            throw new InvalidOperationException("Unknown polygon segment type.");
        }

        private void StartSegment(Unit2D startPosition)
        {
            if (!_started)
            {
                _started = true;
                _currentPosition = startPosition;
                _points.Add(new Point(_currentPosition));
            }
        }
    }
    
    public List<Point> Points => _points;

    private readonly List<Point> _points;

    public MarkerPathPointList()
    {
        _points = new();
    }

    public void CalculatePath(Polygon polygon, Unit spacing, Unit offset)
    {
        var distanceWalker = new CapDistanceWalker();

        distanceWalker.Reset(offset);

        polygon.Resolver.WalkPolygon(distanceWalker);

        var startPoint = distanceWalker.Point;

        var markerPathWalker = new MarkerPathWalker(spacing, offset);
        var walker = new ClampedGeometryWalker(markerPathWalker);

        walker.SetStartEnd(startPoint, null);
        
        polygon.Resolver.WalkPolygon(walker);
        
        _points.Clear();
        _points.AddRange(markerPathWalker.Points);
    }
}
