using StencilPad.Spatial;

public struct Bezier2D
{
    public Unit2D P0 => _p0;
    public Unit2D P1 => _p1;
    public Unit2D P2 => _p2;
    public Unit2D P3 => _p3;

    public Bezier X => new(P0.X, P1.X, P2.X, P3.X);
    public Bezier Y => new(P0.Y, P1.Y, P2.Y, P3.Y);
    
    private Unit2D _p0;
    private Unit2D _p1;
    private Unit2D _p2;
    private Unit2D _p3;

    public Bezier2D(Unit2D p0,
                    Unit2D p1,
                    Unit2D p2,
                    Unit2D p3)
    {
        _p0 = p0;
        _p1 = p1;
        _p2 = p2;
        _p3 = p3;
    }

    public Unit2D At(double t)
    {
        double t_2 = t * t;
        double t_3 = t * t * t;
        double mt = 1 - t;
        double mt_2 = mt * mt;
        double mt_3 = mt * mt * mt;

        // B(t) = (1-t)^3 * P0 + 3(1-t)^2 * t * P1 + 3(1-t) * t^2 * P2 + t^3 * P3
        
        return (mt_3 * _p0) + (3 * mt_2 * t * _p1) + (3 * mt * t_2 * _p2) + (t_3 * _p3);
    }

	public Unit2D Deriv(double t)
	{
		double t_2 = t * t;
		double mt = 1 - t;
		double mt_2 = mt * mt;

        // B'(t) = 3(1-t)^2 * (P1 - P0) + 6(1-t) * t * (P2 - P1) + 3t^2 * (P3 - P2)
        
		return 3 * mt_2 * (_p1 - _p0) + 6 * mt * t * (_p2 - _p1) + 3 * t_2 * (_p3 - _p2);
	}
    
    public double Walk(double start,
                       double end,
                       double step,
                       double minStep,
                       Unit length,
                       Unit tolerance)
    {
        var currentPosition = At(start);
        
        while (Iterate(start, end, step, minStep, tolerance, out double next))
        {
            var nextPosition = At(next);
            var segmentLength = (nextPosition - currentPosition).Magnitude;

            if (Unit.Abs(segmentLength - length) <= tolerance)
            {
                return next;
            }
            else if (segmentLength < length)
            {
                start = next;
                length -= segmentLength;
                currentPosition = nextPosition;
            }
            else
            {
                return length / segmentLength * (next - start) + start;
            }
        }

        return end;
    }
    
    public bool WalkRadius(double start,
                           double end,
                           double step,
                           double minStep,
                           Unit radius,
                           Unit tolerance,
                           out double t)
    {
        var initialPosition = At(start);
        var currentRadius = Unit.Zero;
        
        while (Iterate(start, end, step, minStep, tolerance, out double next))
        {
            var nextPosition = At(next);
            var nextRadius = (nextPosition - initialPosition).Magnitude;
            
            if ((radius >= currentRadius && radius <= nextRadius)
                || (radius >= nextRadius && radius <= currentRadius))
            {
                t = Double.Lerp(start, next, Unit.InverseLerp(currentRadius, nextRadius, radius));
                
                return true;
            }
            
            start = next;
            currentRadius = nextRadius;
        }

        t = default;
        
        return false;
    }

    private bool Iterate(double start,
                         double end,
                         double step,
                         double minStep,
                         Unit tolerance,
                         out double t)
    {
        if (step > 0 && start >= end)
        {
            t = end;
            
            return false;
        }

        if (step < 0 && start <= end)
        {
            t = end;
            
            return false;
        }
        
        double next = (step > 0) ? Math.Min(start + step, end) : Math.Max(start + step, end);
        double mid = (start + next) / 2.0;
        var lenA = (At(next) - At(start)).Magnitude;
        var lenB = (At(next) - At(mid)).Magnitude + (At(mid) - At(start)).Magnitude;
        
        if (Unit.Abs(lenA - lenB) <= tolerance || Math.Abs(step) <= Math.Abs(minStep))
        {
            t = next;
            
            return true;
        }

        return Iterate(start, end, step / 2.0, minStep, tolerance, out t);
    }

    public override string ToString()
    {
        return $"[P0: {P0}, P1: {P1}, P2: {P2}, P3: {P3}]";
    }
}
