namespace StencilPad.Spatial;

public struct Arc
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
    
    private Unit2D _center;
    private Unit _radius;
    private double _startAngle;
    private double _endAngle;

    public Arc(Unit2D start, Unit2D mid, Unit2D end)
    {
        (_center, _radius) = MathUtil.CircleFromArc(start, mid, end);

        _startAngle = Math.Atan2((start.Y - _center.Y).Millimeters,
                                 (start.X - _center.X).Millimeters);

        _endAngle = Math.Atan2((end.Y - _center.Y).Millimeters,
                               (end.X - _center.X).Millimeters);

    }

    public override string ToString()
    {
        return $"[Center={Center}, Radius={Radius}, StartAngle={StartAngle * MathUtil.Rad2Deg}, EndAngle={EndAngle * MathUtil.Rad2Deg}]";
    }
}
