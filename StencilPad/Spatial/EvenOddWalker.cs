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

    public bool Line(int segmentIndex, Unit2D from, Unit2D to)
    {
        if (IntersectsLine(new Line(from, to), _point))
        {
            ++_count;
        }

        return true;
    }

    public bool Arc(int segmentIndex, Arc arc)
    {
        _count += IntersectsArc(arc, _point);

        return true;
    }

    public bool Bezier(int segmentIndex, Bezier2D bezier)
    {
        _count += IntersectsBezier(bezier, _point);

        return true;
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
        int count = 0;
        
        var lineStart = point;
        var lineEnd = new Unit2D(point.X + Unit.FromMillimeters(1000000), point.Y);
        var arcRange = MathUtil.AngleDifference(arc.EndAngle, arc.StartAngle);

        var (i0, i1) = MathUtil.GetCircleLineIntersection(arc.Center,
                                                          arc.Radius,
                                                          lineStart,
                                                          lineEnd);

        if (i0 is not null)
        {
            var angle = Math.Atan2(i0.Value.Y.Millimeters - arc.Center.Y.Millimeters,
                                   i0.Value.X.Millimeters - arc.Center.X.Millimeters);

            if (MathUtil.AngleDifference(angle, arc.StartAngle) <= arcRange &&
                MathUtil.AngleDifference(angle, arc.EndAngle) <= arcRange)
            {
                ++count;
            }
        }
        
        if (i1 is not null)
        {
            var angle = Math.Atan2(i1.Value.Y.Millimeters - arc.Center.Y.Millimeters,
                                   i1.Value.X.Millimeters - arc.Center.X.Millimeters);

            if (MathUtil.AngleDifference(angle, arc.StartAngle) <= arcRange &&
                MathUtil.AngleDifference(angle, arc.EndAngle) <= arcRange)
            {
                ++count;
            }
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
