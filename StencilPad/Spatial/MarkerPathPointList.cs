using StencilPad.Spatial;

public class MarkerPathPointList
{
    public class MarkerPathWalker : IGeometryWalker
    {
        public IEnumerable<UnitTransform> Points => _points;

        private readonly Unit _spacing;
        private readonly Unit _offset;
        
        private bool _started;
        private Unit2D _currentPosition;
        private List<UnitTransform> _points;

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

        private bool WalkLine(Line line)
        {
            StartSegment(line.Start, line.Deriv(0));
            
            return Walk(t => line.FromRadius(_currentPosition, _spacing, t, 1.0),
                        t => (line.At(t), line.Deriv(t)));
        }
        
        private bool WalkArc(Arc arc)
        {
            StartSegment(arc.Start, arc.Deriv(0));
            
            return Walk(t => arc.FromRadius(_currentPosition, _spacing, t, 1.0),
                        t => (arc.At(t), arc.Deriv(t)));
        }

        private bool WalkBezier(Bezier2D bezier)
        {
            Unit tolerance = Unit.FromMillimeters(0.000001);
            double step = 0.1;
            double minStep = 0.0001;

            StartSegment(bezier.P0, bezier.Deriv(0));
            
            return Walk(t => bezier.WalkRadius(_currentPosition, t, 1.0, step, minStep, _spacing, tolerance),
                        t => (bezier.At(t), bezier.Deriv(t)));
        }
        
        private bool Walk(Func<double, double?> fromRadius, Func<double, (Unit2D, Unit2D)> at)
        {
            double lastT = 0;

            while (true)
            {
                var nextT = fromRadius(lastT);

                if (nextT is null)
                {
                    break;
                }

                var (point, deriv) = at(nextT.Value);

                AddPoint(point, deriv);

                _currentPosition = point;
                lastT = nextT.Value;
            }

            return true;
        }

        private void StartSegment(Unit2D startPosition, Unit2D startDirection)
        {
            if (!_started)
            {
                _started = true;
                _currentPosition = startPosition;
                AddPoint(startPosition, startDirection);
            }
        }

        private void AddPoint(Unit2D position, Unit2D direction)
        {
            var angle = Math.Atan2(direction.Y.Millimeters, direction.X.Millimeters);
            
            _points.Add(new UnitTransform(position, (decimal)(angle * MathUtil.Rad2Deg) + 90));
        }
    }
    
    public List<UnitTransform> Points => _points;

    private readonly List<UnitTransform> _points;

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
