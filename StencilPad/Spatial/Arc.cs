namespace StencilPad.Spatial;

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
    
    public Arc Subsegment(double start, double end)
    {
        var startAngle = start <= 0.0 ? _startAngle : MathUtil.LerpAngle(_startAngle, _endAngle, start);
        var endAngle   = end   >= 1.0 ? _endAngle   : MathUtil.LerpAngle(_startAngle, _endAngle, end);
        return new Arc(_center, _radius, startAngle, endAngle);
    }

    public override string ToString()
    {
        return $"[Center={Center}, Radius={Radius}, StartAngle={StartAngle * MathUtil.Rad2Deg}, EndAngle={EndAngle * MathUtil.Rad2Deg}]";
    }
}
