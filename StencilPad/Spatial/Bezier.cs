using StencilPad.Spatial;

public struct Bezier
{
    public Unit P0 => _p0;
    public Unit P1 => _p1;
    public Unit P2 => _p2;
    public Unit P3 => _p3;
    
    private Unit _p0;
    private Unit _p1;
    private Unit _p2;
    private Unit _p3;

    public Bezier(Unit p0,
                  Unit p1,
                  Unit p2,
                  Unit p3)
    {
        _p0 = p0;
        _p1 = p1;
        _p2 = p2;
        _p3 = p3;
    }

    public Unit At(double t)
    {
        double t_2 = t * t;
        double t_3 = t * t * t;
        double mt = 1 - t;
        double mt_2 = mt * mt;
        double mt_3 = mt * mt * mt;

        // B(t) = (1-t)^3 * P0 + 3(1-t)^2 * t * P1 + 3(1-t) * t^2 * P2 + t^3 * P3
        
        return (mt_3 * _p0) + (3 * mt_2 * t * _p1) + (3 * mt * t_2 * _p2) + (t_3 * _p3);
    }

	public Unit Deriv(double t)
	{
		double t_2 = t * t;
		double mt = 1 - t;
		double mt_2 = mt * mt;

        // B'(t) = 3(1-t)^2 * (P1 - P0) + 6(1-t) * t * (P2 - P1) + 3t^2 * (P3 - P2)
        
		return 3 * mt_2 * (_p1 - _p0) + 6 * mt * t * (_p2 - _p1) + 3 * t_2 * (_p3 - _p2);
	}

    public (double, double)? CalculateExtrema()
    {
        // The extrema of a bezier are the two values where B'(t) = 0 (ie the
        // gradient is flat).
        //
        // So we take the derivative of the bezier equation, which is;
        // B'(t) = 3(1-t)^2 * (P1 - P0) + 6(1-t) * t * (P2 - P1) + 3t^2 * (P3 - P2)
        //
        // And we can rearrange it such that;
        // B'(t) = At^2 + Bt + C
        //
        // where;
        // A = 3(-P0 + 3P1 - 3P2 + P3)
        // B = 6(P0 - 2P1 + P2)
        // C = 3(P1 - P0)
        //
        // Which is a quadratic equation where we want to solve for;
        // At^2 + Bt + C = 0
        //
        // So we solve it using the quadratic formula, which is;
        // t = (-B +- sqrt(B^2 - 4AC)) / 2A
        //
        // Convert to millimeters because these are scalar values and the Unit
        // semantics for them don't exist.
        
        var a = (3 * (-_p0 + 3 * _p1 - 3 * _p2 + _p3)).Millimeters;
        var b = (6 * (_p0 - 2 * _p1 + _p2)).Millimeters;
        var c = (3 * (_p1 - _p0)).Millimeters;

        var discriminant = (b * b) - (4 * a * c);

        // If B^2 - 4AC is negative then this is going to be an imaginary
        // number, which means there are no real roots and thus no extrema.

        if (discriminant < 0)
        {
            return null;
        }

        var sqrtDiscriminant = Math.Sqrt(discriminant);

        // The two T values which correspond to our extrema are;
        // (-B + sqrt(B^2 - 4AC)) / 2A
        // and
        // (-B - sqrt(B^2 - 4AC)) / 2A
        
        return (
            (-b + sqrtDiscriminant) / (2 * a),
            (-b - sqrtDiscriminant) / (2 * a)
        );
    }
}
