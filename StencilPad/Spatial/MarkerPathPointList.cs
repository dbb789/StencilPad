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
        
        public bool Line(int segmentIndex, Line line)
        {
            var from = line.Start;
            var to = line.End;
            
            StartSegment(from);
            
            double lastT = -1;

            while (true)
            {
                var (t0, t1) = MathUtil.GetCircleLineIntersectionFractions(_currentPosition,
                                                                           _spacing,
                                                                           line);

                // NOTE: MathUtil.SolveQuadratic() is guaranteed to return with t1 > t0.
                if (t0 is not null && t0.Value > lastT)
                {
                    var point = from + ((to - from) * t0.Value);

                    _points.Add(new Point(point));

                    lastT = t0.Value;
                    
                    _currentPosition = point;
                }
                else if (t1 is not null && t1.Value > lastT)
                {
                    var point = from + ((to - from) * t1.Value);

                    _points.Add(new Point(point));

                    lastT = t1.Value;

                    _currentPosition = point;
                }
                else
                {
                    break;
                }
            }
            
            return true;
        }
        
        public bool Arc(int segmentIndex, Arc arc)
        {
            StartSegment(arc.Start);

            // var (arcCenter, arcRadius) = MathUtil.CircleFromArc(start, mid, end);
            // var startAngle = Math.Atan2((start.Y - arcCenter.Y).Millimeters, (start.X - arcCenter.X).Millimeters);
            // var endAngle = Math.Atan2((end.Y - arcCenter.Y).Millimeters, (end.X - arcCenter.X).Millimeters);
            // var arcAngle = MathUtil.SignedAngleDifference(startAngle, endAngle);

            // System.Diagnostics.Debug.WriteLine("////////////////////////////////////////");
            // System.Diagnostics.Debug.WriteLine($"arcAngle : {arcAngle}");
            
            // double currentT = -1;

            // while (true)
            // {
            //     var (a, b) = MathUtil.GetCircleCircleIntersection(arcCenter, arcRadius, _currentPosition, _spacing);

            //     var tA = GetArcFraction(a, arcCenter, startAngle, arcAngle);
            //     var tB = GetArcFraction(b, arcCenter, startAngle, arcAngle);

            //     if (currentT >= 0 && tA <= currentT)
            //     {
            //         tA = null;
            //     }

            //     if (currentT >= 0 && tB <= currentT)
            //     {
            //         tB = null;
            //     }
                
            //     double nextT;
            //     Unit2D nextPoint;
                
            //     if (tA is null && tB is null)
            //     {
            //         break;
            //     }
            //     else if (tB is null)
            //     {
            //         nextT = tA!.Value;
            //         nextPoint = a!.Value;
            //     }
            //     else if (tA is null)
            //     {
            //         nextT = tB.Value;
            //         nextPoint = b.Value;
            //     }
            //     else
            //     {
            //         nextT = tA.Value < tB.Value ? tA.Value : tB.Value;
            //         nextPoint = tA.Value < tB.Value ? a!.Value : b!.Value;
            //     }
                
            //     _points.Add(new Point(nextPoint));

            //     _currentPosition = nextPoint;
            //     currentT = nextT;
            // }
            
            return true;
        }

        private double? GetArcFraction(Unit2D? point, Unit2D arcCenter, double startAngle, double arcAngle)
        {
            if (point is null)
            {
                return null;
            }

            var angle = Math.Atan2((point.Value.Y - arcCenter.Y).Millimeters, (point.Value.X - arcCenter.X).Millimeters);
            double t = MathUtil.SignedAngleDifference(startAngle, angle) / arcAngle;

            if (t >= 0 && t <= 1)
            {
                return t;
            }

            return null;
        }

        public bool Bezier(int segmentIndex, Bezier2D bezier)
        {
            Unit tolerance = Unit.FromMillimeters(0.000001);
            double step = 0.1;
            double minStep = 0.0001;
            
            StartSegment(bezier.P0);

            double t = 0;

            while (bezier.WalkRadius(_currentPosition,
                                     t,
                                     1.0,
                                     step,
                                     minStep,
                                     _spacing,
                                     tolerance,
                                     out var nextT))
            {
                var point = bezier.At(nextT);

                _points.Add(new Point(point));

                _currentPosition = point;
                t = nextT;
            }
            
            return true;
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
