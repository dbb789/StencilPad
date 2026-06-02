using StencilPad.Spatial;

public struct Bezier2D
{
    public class IterateArgs
    {
        public double InitialStep { get; init; }
        public double MaxStep { get; init; }
        public double MinStep { get; init; }
        public Unit Tolerance { get; init; }
    }

    public static readonly IterateArgs IterateFine = new()
    {
        InitialStep = 0.1,
        MaxStep = 0.1,
        MinStep = 0.0001,
        Tolerance = Unit.FromMillimeters(0.000001)
    };
    
    public static readonly IterateArgs IterateCoarse = new()
    {
        InitialStep = 0.25,
        MaxStep = 0.25,
        MinStep = 0.01,
        Tolerance = Unit.FromMillimeters(0.01)
    };
    
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
        double t_3 = t * t_2;
        double mt = 1 - t;
        double mt_2 = mt * mt;
        double mt_3 = mt * mt_2;

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
    
    public (double, Unit) Walk(double start,
                               double end,
                               Unit maxLength,
                               IterateArgs iterateArgs)
    {
        var remainingLength = maxLength;
        var currentPosition = At(start);

        double step = iterateArgs.InitialStep;
        
        while (Iterate(start, end, iterateArgs, ref step, out double next))
        {
            var nextPosition = At(next);
            var segmentLength = (nextPosition - currentPosition).Magnitude;

            if (Unit.Abs(segmentLength - remainingLength) <= iterateArgs.Tolerance)
            {
                // Segment and remaining length are close enough - return the end of the segment.
                return (next, maxLength);
            }
            else if (segmentLength < remainingLength)
            {
                // Segment is shorter than remaining length - move to the end of the segment and continue.
                start = next;
                remainingLength -= segmentLength;
                currentPosition = nextPosition;
            }
            else
            {
                // Segment is longer than remaining length - we've overshot due
                // to Iterate() tolerance, so we need to estimate the position
                // along the segment that corresponds to the total length.
                
                var fraction = remainingLength / segmentLength;
                var estimatedPoint = Double.Lerp(start, next, fraction);

                return (estimatedPoint, maxLength);
            }
        }

        // Exceeded end - return the end position and the length we actually walked.
        return (end, maxLength - remainingLength);
    }
    
    public double? WalkRadius(double start,
                              double end,
                              Unit radius,
                              IterateArgs iterateArgs)
    {
        return WalkRadius(At(start), start, end, radius, iterateArgs);
    }

    public double? WalkRadius(Unit2D initialPosition,
                              double start,
                              double end,
                              Unit radius,
                              IterateArgs iterateArgs)
    {
        var currentRadius = Unit.Zero;

        double step = iterateArgs.InitialStep;
        
        while (Iterate(start, end, iterateArgs, ref step, out double next))
        {
            var nextPosition = At(next);
            var nextRadius = (nextPosition - initialPosition).Magnitude;
            
            if ((radius >= currentRadius && radius <= nextRadius)
                || (radius >= nextRadius && radius <= currentRadius))
            {
                return Double.Lerp(start, next, Unit.InverseLerp(currentRadius, nextRadius, radius));
            }
            
            start = next;
            currentRadius = nextRadius;
        }

        return null;
    }
    
    public bool Iterate(double start,
                        double end,
                        IterateArgs iterateArgs,
                        ref double step,
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

        var startPoint = At(start);
            
        while (true)
        {
            double next = (step > 0) ? Math.Min(start + step, end) : Math.Max(start + step, end);
            double mid = (start + next) / 2.0;

            var nextPoint = At(next);
            var midPoint = At(mid);
            
            var lenA = (nextPoint - startPoint).Magnitude;
            var lenB = (nextPoint - midPoint).Magnitude + (midPoint - startPoint).Magnitude;

            var error = Unit.Abs(lenA - lenB);

            // If the error is significantly smaller than the tolerance, we
            // increase the step size.
            if (error <= (iterateArgs.Tolerance / 8))
            {
                t = next;
                step = Math.Min(step * 2.0, iterateArgs.MaxStep);

                return true;
            }

            // This is deliberately before the conditional - it saves us one additional iteration.
            step /= 2.0;
            
            if (error <= iterateArgs.Tolerance || Math.Abs(step) <= Math.Abs(iterateArgs.MinStep))
            {
                t = next;
                
                return true;
            }
        }
    }

    // De Casteljau's algorithm.
    public Bezier2D SplitLeft(double t)
    {
        var p01 = Unit2D.Lerp(P0, P1, t);
        var p12 = Unit2D.Lerp(P1, P2, t);
        var p23 = Unit2D.Lerp(P2, P3, t);
        var p012 = Unit2D.Lerp(p01, p12, t);
        var p123 = Unit2D.Lerp(p12, p23, t);
        var p0123 = Unit2D.Lerp(p012, p123, t);

        return new Bezier2D(P0, p01, p012, p0123);
    }

    public Bezier2D SplitRight(double t)
    {
        var p01 = Unit2D.Lerp(P0, P1, t);
        var p12 = Unit2D.Lerp(P1, P2, t);
        var p23 = Unit2D.Lerp(P2, P3, t);
        var p012 = Unit2D.Lerp(p01, p12, t);
        var p123 = Unit2D.Lerp(p12, p23, t);
        var p0123 = Unit2D.Lerp(p012, p123, t);

        return new Bezier2D(p0123, p123, p23, P3);
    }
    
    public Bezier2D Subsegment(double start, double end)
    {
        var bezier = this;

        if (start > 0.0)
        {
            bezier = bezier.SplitRight(start);

            if (end < 1.0)
            {
                bezier = bezier.SplitLeft((end - start) / (1.0 - start));
            }
        }
        else if (end < 1.0)
        {
            bezier = bezier.SplitLeft(end);
        }

        return bezier;
    }

    public override string ToString()
    {
        return $"[P0: {P0}, P1: {P1}, P2: {P2}, P3: {P3}]";
    }
}
