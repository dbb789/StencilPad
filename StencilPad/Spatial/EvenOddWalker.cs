namespace StencilPad.Spatial;

public class EvenOddWalker : IGeometryWalker
{
    public bool Hit => _hit || (_count % 2) == 1;

    private readonly Unit2D _point;
    private readonly Unit _halfThickness;
    private int _count;
    private bool _hit;

    public EvenOddWalker(Unit2D point, Unit lineWidth)
    {
        _point = point;
        _halfThickness = lineWidth / 2;

        Reset();
    }

    public void Reset()
    {
        _count = 0;
        _hit = false;
    }

    public bool Begin(int segmentCount, bool closed)
    {
        return true;
    }

    public bool Segment(int segmentIndex, PolygonSegment segment)
    {
        if (segment.IsLine)
        {
            return TestLine(segment.Line);
        }

        if (segment.IsArc)
        {
            return TestArc(segment.Arc);
        }

        if (segment.IsBezier)
        {
            return TestBezier(segment.Bezier);
        }

        throw new InvalidOperationException("Unknown polygon segment type.");
    }

    public bool AddLine(Unit2D from, Unit2D to)
    {
        return TestLine(new Line(from, to));
    }

    private bool TestLine(Line line)
    {        
        if (line.DistanceTo(_point) <= _halfThickness)
        {
            _hit = true;
            
            return false;
        }

        if (IntersectsLine(line, _point))
        {
            ++_count;
        }

        return true;
    }

    private bool TestArc(Arc arc)
    {
        if (arc.DistanceTo(_point) <= _halfThickness)
        {
            _hit = true;
            return false;
        }

        var ray = new Line(_point, new Unit2D(_point.X + Unit.MaxValue, _point.Y));

        var (t0, t1) = arc.Intersection(ray);

        if (t0 is not null)
        {
            ++_count;
        }

        if (t1 is not null)
        {
            ++_count;
        }
        
        return true;
    }

    private bool TestBezier(Bezier2D bezier)
    {
        double t = 0;
        double step = Bezier2D.IterateCoarse.InitialStep;

        while (bezier.Iterate(t, 1, Bezier2D.IterateCoarse, ref step, out double next))
        {
            var line = new Line(bezier.At(t), bezier.At(next));

            if (line.DistanceTo(_point) <= _halfThickness)
            {
                _hit = true;
                
                return false;
            }

            if (IntersectsLine(line, _point))
            {
                ++_count;
            }

            t = next;
        }

        return true;
    }

    private static bool IntersectsLine(Line line, Unit2D point)
    {
        double ax = line.Start.X.Millimeters;
        double ay = line.Start.Y.Millimeters;
        double bx = line.End.X.Millimeters;
        double by = line.End.Y.Millimeters;
        double px = point.X.Millimeters;
        double py = point.Y.Millimeters;

        double cross = (bx - ax) * (py - ay) - (by - ay) * (px - ax);

        if (ay <= py)
        {
            if (by > py && cross > 0)
            {
                return true;
            }
        }
        else
        {
            if (by <= py && cross < 0)
            {
                return true;
            }
        }

        return false;
    }
}

