namespace StencilPad.Spatial;

// NOTE: Arcs in this system are purely for rendering circular corners, and as
// such are guaranteed to be spherical and will never exceed 180 degrees.
public readonly struct Arc
{
    public Unit2D Center => _center;
    public Unit Radius => _radius;
    public double StartAngle => _startAngle;
    public double EndAngle => _endAngle;

    public Unit2D Start
    {
        get
        {
            return new Unit2D(_center.X + _radius * Math.Cos(_startAngle),
                              _center.Y + _radius * Math.Sin(_startAngle));
        }
    }

    public Unit2D End
    {
        get
        {
            return new Unit2D(_center.X + _radius * Math.Cos(_endAngle),
                              _center.Y + _radius * Math.Sin(_endAngle));
        }
    }

    public Unit Length
    {
        get
        {
            var angleDiff = MathUtil.AngleDifference(_endAngle, _startAngle);
            
            return Unit.FromMillimeters(Math.Abs(angleDiff) * _radius.Millimeters);
        }
    }
    
    private readonly Unit2D _center;
    private readonly Unit _radius;
    private readonly double _startAngle;
    private readonly double _endAngle;

    public Arc(Unit2D start, Unit2D mid, Unit2D end)
    {
        (_center, _radius) = MathUtil.CircleFromArc(start, mid, end);

        _startAngle = Math.Atan2((start.Y - _center.Y).Millimeters,
                                 (start.X - _center.X).Millimeters);

        _endAngle = Math.Atan2((end.Y - _center.Y).Millimeters,
                               (end.X - _center.X).Millimeters);

    }

    public Arc(Unit2D center, Unit radius, double startAngle, double endAngle)
    {
        _center = center;
        _radius = radius;
        _startAngle = startAngle;
        _endAngle = endAngle;
    }

    public double? FromRadius(Unit2D startPoint, Unit radius, double start, double end)
    {
        var arcAngle = MathUtil.SignedAngleDifference(_startAngle, _endAngle);
        var (a, b) = MathUtil.GetCircleCircleIntersection(_center, _radius, startPoint, radius);

        var tA = ToFraction(a, arcAngle);
        var tB = ToFraction(b, arcAngle);

        if (tA < start || tA > end)
        {
            tA = null;
        }

        if (tB < start || tB > end)
        {
            tB = null;
        }

        if (tA is null && tB is null)
        {
            return null;
        }

        if (tA is null)
        {
            return tB;
        }

        if (tB is null)
        {
            return tA;
        }
        
        return Math.Min(tA.Value, tB.Value);
    }
    
    private double? ToFraction(Unit2D? point, double arcAngle)
    {
        if (point is null)
        {
            return null;
        }
        
        var angle = Math.Atan2((point.Value.Y - _center.Y).Millimeters,
                               (point.Value.X - _center.X).Millimeters);
        double t = MathUtil.SignedAngleDifference(_startAngle, angle) / arcAngle;

        return t >= 0 && t <= 1 ? t : null;
    }

    public Unit2D At(double t)
    {
        var angle = MathUtil.LerpAngle(_startAngle, _endAngle, t);
        
        return new Unit2D(_center.X + _radius * Math.Cos(angle),
                          _center.Y + _radius * Math.Sin(angle));
    }

    public Unit2D Deriv(double t)
    {
        var angle = MathUtil.LerpAngle(_startAngle, _endAngle, t);
        
        return new Unit2D(-Math.Sin(angle) * _radius,
                          Math.Cos(angle) * _radius);
    }
    
    public (double?, double?) Intersection(Line line)
    {
        var arcRange = MathUtil.AngleDifference(EndAngle, StartAngle);

        var (i0, i1) = MathUtil.GetCircleLineIntersection(Center,
                                                          Radius,
                                                          line);

        double? t0 = null;
        double? t1 = null;
        
        if (i0 is not null)
        {
            var angle = Math.Atan2(i0.Value.Y.Millimeters - Center.Y.Millimeters,
                                   i0.Value.X.Millimeters - Center.X.Millimeters);

            t0 = MathUtil.InverseLerpAngle(_startAngle, _endAngle, angle);
        }
        
        if (i1 is not null)
        {
            var angle = Math.Atan2(i1.Value.Y.Millimeters - Center.Y.Millimeters,
                                   i1.Value.X.Millimeters - Center.X.Millimeters);

            t1 = MathUtil.InverseLerpAngle(_startAngle, _endAngle, angle);
        }

        if (t0 is not null && (t0 < 0 || t0 > 1))
        {
            t0 = null;
        }

        if (t1 is not null && (t1 < 0 || t1 > 1))
        {
            t1 = null;
        }
        
        return (t0, t1);
    }

    public Unit DistanceTo(Unit2D point)
    {
        double theta = Math.Atan2((point.Y - _center.Y).Millimeters,
                                  (point.X - _center.X).Millimeters);

        double arcAngle = MathUtil.SignedAngleDifference(_startAngle, _endAngle);
        double t = MathUtil.SignedAngleDifference(_startAngle, theta) / arcAngle;

        if (t >= 0.0 && t <= 1.0)
        {
            double dx = (point.X - _center.X).Millimeters;
            double dy = (point.Y - _center.Y).Millimeters;
            double distFromCenter = Math.Sqrt(dx * dx + dy * dy);

            return Unit.FromMillimeters(Math.Abs(distFromCenter - _radius.Millimeters));
        }

        var toStart = (point - Start).Magnitude;
        var toEnd   = (point - End).Magnitude;

        return toStart < toEnd ? toStart : toEnd;
    }

    public Arc Subsegment(double start, double end)
    {
        var startAngle = (start <= 0.0) ? _startAngle : MathUtil.LerpAngle(_startAngle, _endAngle, start);
        var endAngle = (end >= 1.0) ? _endAngle : MathUtil.LerpAngle(_startAngle, _endAngle, end);
        
        return new Arc(_center, _radius, startAngle, endAngle);
    }

    public override string ToString()
    {
        return $"[Center={Center}, Radius={Radius}, StartAngle={StartAngle * MathUtil.Rad2Deg}, EndAngle={EndAngle * MathUtil.Rad2Deg}]";
    }
}
