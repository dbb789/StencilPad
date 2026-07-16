namespace StencilPad.Spatial;

public static class MathUtil
{
    public const double Deg2Rad = Math.PI / 180.0;
    public const double Rad2Deg = 180.0 / Math.PI;
    public const double Epsilon = 1e-10;
    public const decimal Kappa = 0.5522847498307933984022516322796m;

    public static (double?, double?) GetCircleLineIntersectionFractions(Unit2D center,
                                                                        Unit radius,
                                                                        Line line)
    {
        radius = Unit.Abs(radius);
        
        Unit2D d = line.End - line.Start;
        double dx = d.X.Millimeters;
        double dy = d.Y.Millimeters;
        double centerX = center.X.Millimeters;
        double centerY = center.Y.Millimeters;
        double radiusMm = radius.Millimeters;
        double p0X = line.Start.X.Millimeters;
        double p0Y = line.Start.Y.Millimeters;

        double a = d.SqrMagnitude;
        double b = 2 * (dx * (p0X - centerX) + dy * (p0Y - centerY));
        double c = (p0X - centerX) * (p0X - centerX) + (p0Y - centerY) * (p0Y - centerY) - (radiusMm * radiusMm);

        return SolveQuadratic01(a, b, c);
    }
    
    public static (Unit2D?, Unit2D?) GetCircleLineIntersection(Unit2D center,
                                                               Unit radius,
                                                               Line line)
    {
        var (t0, t1) = GetCircleLineIntersectionFractions(center, radius, line);
        
        Unit2D? i0 = null;
        Unit2D? i1 = null;

        if (t0 is not null)
        {
            i0 = line.At(t0.Value);
        }

        if (t1 is not null)
        {
            i1 = line.At(t1.Value);
        }
        
        return (i0, i1);
    }
    
    public static (Unit2D center, Unit radius) CircleFromArc(Unit2D start, Unit2D mid, Unit2D end)
    {
        var offsetA = start - mid;
        var offsetB = end - mid;
        var angle = Unit2D.SignedAngle(offsetA, offsetB);
        var radius = Unit.Min(offsetA.Magnitude, offsetB.Magnitude) * Math.Tan(Math.Abs(angle) / 2.0);
        
        var chordMid = (end + start) / 2;
        var diagonal = (end - start).Magnitude / 2.0;

        var sqrCenterDistance = (radius.Millimeters * radius.Millimeters) - (diagonal.Millimeters * diagonal.Millimeters);
        var centerDistance = Unit.FromMillimeters(Math.Sqrt(Math.Abs(sqrCenterDistance)));

        if (sqrCenterDistance < 0)
        {
            return (chordMid, radius);
        }
        
        var centerDirection = chordMid - mid;
        var center = chordMid + centerDirection.NormalizedTo(centerDistance);

        return (center, radius);
    }

    public static (Unit2D?, Unit2D?) GetCircleCircleIntersection(Unit2D c0, Unit r0, Unit2D c1, Unit r1)
    {
        double dx = (c1.X - c0.X).Millimeters;
        double dy = (c1.Y - c0.Y).Millimeters;
        double d2 = dx*dx + dy*dy;
        double d = Math.Sqrt(d2);
        double r0mm = r0.Millimeters;
        double r1mm = r1.Millimeters;

        if (d < Epsilon || d > r0mm + r1mm || d < Math.Abs(r0mm - r1mm))
        {
            return (null, null);
        }
        
        double a = (r0mm*r0mm - r1mm*r1mm + d2) / (2 * d);
        double h2 = r0mm*r0mm - a*a;

        if (h2 < 0)
        {
            return (null, null);
        }
        
        double px = c0.X.Millimeters + a * dx / d;
        double py = c0.Y.Millimeters + a * dy / d;

        if (h2 < Epsilon)
        {
            return (Unit2D.FromMillimeters(px, py), null);
        }
        
        double h = Math.Sqrt(h2);
        double perpX = -dy / d;
        double perpY =  dx / d;

        return (Unit2D.FromMillimeters(px + h * perpX, py + h * perpY),
                Unit2D.FromMillimeters(px - h * perpX, py - h * perpY));
    }

    public static (double?, double?) SolveQuadratic(double a, double b, double c)
    {
        // When a is zero this isn't a quadratic equation, it's a linear one ie;
        // Ax^2 + Bx + C = 0 where A = 0 gives us Bx + C = 0, which we can
        // rearrange to x = -C / B.
        //
        // This can occur (for example) in a perfectly symmetrical bezier where
        // the control points are perfectly in line with the start and end
        // points, which means the derivative is a linear function rather than a
        // quadratic one.
        if (Math.Abs(a) < Epsilon)
        {
            // if B = 0 then that rearranges to C = 0, which is only true when C
            // is also zero, which means every value of x is a solution. In this
            // case we return null for both values to indicate that there are
            // either no solutions or infinite solutions.
            if (Math.Abs(b) < Epsilon)
            {
                return (null, null);
            }
            
            return (-c / b, null);
        }
        
        // t = (-b +- sqrt(b^2 - 4ac)) / 2a
        
        double discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            return (null, null);
        }

        if (discriminant == 0)
        {
            return (-b / (2 * a), null);
        }
        
        var sqrtDiscriminant = Math.Sqrt(discriminant);
        var t0 = (-b + sqrtDiscriminant) / (2 * a);
        var t1 = (-b - sqrtDiscriminant) / (2 * a);

        if (t0 > t1)
        {
            (t0, t1) = (t1, t0);
        }
        
        return (t0, t1);
    }

    public static (double?, double?) SolveQuadratic01(double a, double b, double c)
    {
        var (t0, t1) = SolveQuadratic(a, b, c);

        return (Approximate01(t0), Approximate01(t1));
    }

    // Enforces that t is within the range [0, 1] with a small epsilon
    // tolerance. Returns null if t is outside this range.
    private static double? Approximate01(double? t)
    {
        if (t is null)
        {
            return null;
        }
        
        if (t < 0)
        {
            if (t > -Epsilon)
            {
                return 0;
            }
            else
            {
                return null;
            }
        }

        if (t > 1)
        {
            if (t < 1 + Epsilon)
            {
                return 1;
            }
            else
            {
                return null;
            }
        }

        return t;
    }

    // Normalizes an angle in radians to the range [0, 2 * PI].
    public static double NormalizeAngle(double angleRadians)
    {
        return ((angleRadians % (2 * Math.PI)) + 2 * Math.PI) % (2 * Math.PI);
    }

    // Calculates the signed difference between two angles in radians returning
    // a value in the range [-PI, PI].
    public static double SignedAngleDifference(double a, double b)
    {
        a = NormalizeAngle(a);
        b = NormalizeAngle(b);

        double diff = b - a;

        if (diff > Math.PI)
        {
            diff -= 2 * Math.PI;
        }
        else if (diff < -Math.PI)
        {
            diff += 2 * Math.PI;
        }

        return diff;
    }

    public static double AngleDifference(double a, double b)
    {
        return Math.Abs(SignedAngleDifference(a, b));
    }

    public static double LerpAngle(double a, double b, double t)
    {
        double angleDiff = SignedAngleDifference(a, b);
        
        return a + angleDiff * t;
    }

    public static double InverseLerpAngle(double a, double b, double value)
    {
        double angleDiff = SignedAngleDifference(a, b);
        
        if (Math.Abs(angleDiff) < Epsilon)
        {
            return 0;
        }
        
        double valueDiff = SignedAngleDifference(a, value);
        
        return valueDiff / angleDiff;
    }

    public static Unit2D ControlPointDirection(Unit2D p0, Unit2D p1, Unit2D p2, double strength = 0.25)
    {
        var o0 = p1 - p0;
        var o1 = p2 - p1;
        var offset = Unit2D.Slerp(o0, o1, 0.5);
        
        return offset.NormalizedTo(offset.Magnitude * strength);
    }

    public static Unit2D RemapPoint(Unit2D localPoint,
                                    UnitBounds oldBounds,
                                    UnitBounds newBounds,
                                    UnitTransform transform)
    {
        var worldPoint = transform.Apply(localPoint);
        
        double tX = Unit.InverseLerp(oldBounds.Min.X, oldBounds.Max.X, worldPoint.X);
        double tY = Unit.InverseLerp(oldBounds.Min.Y, oldBounds.Max.Y, worldPoint.Y);

        return transform.InverseApply(new Unit2D(Unit.Lerp(newBounds.Min.X, newBounds.Max.X, tX),
                                                 Unit.Lerp(newBounds.Min.Y, newBounds.Max.Y, tY)));
    }
}
