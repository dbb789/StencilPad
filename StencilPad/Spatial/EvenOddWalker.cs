namespace StencilPad.Spatial;

public class EvenOddWalker : IGeometryWalker
{
    public int Count => _count;

    private readonly Unit2D _point;
    private int _count;

    public EvenOddWalker(Unit2D point)
    {
        _point = point;
        _count = 0;
    }

    public bool Begin(int segmentCount, bool closed)
    {
        return true;
    }

    public bool Segment(int segmentIndex, PolygonSegment segment)
    {
        if (segment.IsLine)
        {
            if (IntersectsLine(segment.Line, _point))
            {
                ++_count;
            }

            return true;
        }

        if (segment.IsArc)
        {
            _count += IntersectsArc(segment.Arc, _point);

            return true;
        }

        if (segment.IsBezier)
        {
            _count += IntersectsBezier(segment.Bezier, _point);

            return true;
        }

        throw new InvalidOperationException("Unknown polygon segment type.");
    }

    public bool AddLine(Unit2D from, Unit2D to)
    {
        if (IntersectsLine(new Line(from, to), _point))
        {
            ++_count;
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
    
    private static int IntersectsArc(Arc arc, Unit2D point)
    {
        var ray = new Line(point, new Unit2D(point.X + Unit.FromMillimeters(1000000),
                                             point.Y));

        var (t0, t1) = arc.Intersection(ray);

        int count = 0;

        if (t0 is not null)
        {
            ++count;
        }

        if (t1 is not null)
        {
            ++count;
        }
        
        return count;
    }

    private static int IntersectsBezier(Bezier2D bezier, Unit2D point)
    {
        int count = 0;
        double t = 0;

        while (bezier.Iterate(t, 1, 0.25, 0.01, Unit.FromMillimeters(0.01), out double next))
        {
            var line = new Line(bezier.At(t), bezier.At(next));

            if (IntersectsLine(line, point))
            {
                ++count;
            }

            t = next;
        }

        return count;
    }
}
