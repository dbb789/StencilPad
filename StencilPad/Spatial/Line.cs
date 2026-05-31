namespace StencilPad.Spatial;

public readonly struct Line
{
    public Unit2D Start => _start;
    public Unit2D End => _end;
    public Unit Length => (_end - _start).Magnitude;

    private readonly Unit2D _start;
    private readonly Unit2D _end;

    public Line(Unit2D start, Unit2D end)
    {
        _start = start;
        _end = end;
    }

    public Unit2D At(double t)
    {
        return Unit2D.Lerp(_start, _end, t);
    }

    public Unit2D Deriv(double t)
    {
        return _end - _start;
    }

    public Unit DistanceTo(Unit2D point)
    {
        double ax = _start.X.Millimeters;
        double ay = _start.Y.Millimeters;
        double bx = _end.X.Millimeters;
        double by = _end.Y.Millimeters;
        double px = point.X.Millimeters;
        double py = point.Y.Millimeters;

        double dx = bx - ax, dy = by - ay;
        double lenSq = dx * dx + dy * dy;

        double t = lenSq > 1e-20
            ? Math.Clamp(((px - ax) * dx + (py - ay) * dy) / lenSq, 0.0, 1.0)
            : 0.0;

        double cx = ax + t * dx, cy = ay + t * dy;

        return Unit.FromMillimeters(Math.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy)));
    }

    public double? FromRadius(Unit2D startPoint, Unit radius, double start, double end)
    {
        var (t0, t1) = MathUtil.GetCircleLineIntersectionFractions(startPoint, radius, this);

        if (t0 < start || t0 > end)
        {
            t0 = null;
        }

        if (t1 < start || t1 > end)
        {
            t1 = null;
        }

        if (t0 is null && t1 is null)
        {
            return null;
        }

        if (t0 is null)
        {
            return t1;
        }

        if (t1 is null)
        {
            return t0;
        }
        
        return Math.Min(t0.Value, t1.Value);
    }

    public Line Subsegment(double start, double end)
    {
        var from = (start <= 0.0) ? _start : Unit2D.Lerp(_start, _end, start);
        var to = (end >= 1.0) ? _end : Unit2D.Lerp(_start, _end, end);
        return new Line(from, to);
    }
}
