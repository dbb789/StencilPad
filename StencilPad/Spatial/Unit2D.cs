using System.Runtime.CompilerServices;
using System.Windows;

namespace StencilPad.Spatial;

public readonly record struct Unit2D(Unit X, Unit Y)
{
    public static readonly Unit2D Zero = new(Unit.Zero, Unit.Zero);

    public Point Millimeters => new(X.Millimeters, Y.Millimeters);

    public Unit Magnitude
    {
        get
        {
            return Unit.FromMillimeters(Math.Sqrt((X.Millimeters * X.Millimeters) + (Y.Millimeters * Y.Millimeters)));
        }
    }
    
    public Unit SqrMagnitude
    {
        get
        {
            return Unit.FromMillimeters((X.Millimeters * X.Millimeters) + (Y.Millimeters * Y.Millimeters));
        }
    }

    public Unit2D Normalized
    {
        get
        {
            var magnitude = Magnitude;
            
            if (magnitude == Unit.Zero)
            {
                return Zero;
            }

            return new Unit2D(Unit.FromMillimeters(X.Millimeters / magnitude.Millimeters),
                              Unit.FromMillimeters(Y.Millimeters / magnitude.Millimeters));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit2D Abs(Unit2D u)
    {
        return new(Unit.Abs(u.X), Unit.Abs(u.Y));
    }
    
    public static Unit2D Square(Unit side)
    {
        return new(side, side);
    }

    public static double Determinant(Unit2D a, Unit2D b)
    {
        return (a.X.Millimeters * b.Y.Millimeters) - (a.Y.Millimeters * b.X.Millimeters);
    }
    
    public static double Dot(Unit2D a, Unit2D b)
    {
        return (a.X.Millimeters * b.X.Millimeters) + (a.Y.Millimeters * b.Y.Millimeters);
    }
        
    public static double SignedAngle(Unit2D a, Unit2D b)
    {
        return Math.Atan2(Determinant(a, b), Dot(a, b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit2D operator +(Unit2D a, Unit2D b) => new(a.X + b.X, a.Y + b.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit2D operator -(Unit2D a, Unit2D b) => new(a.X - b.X, a.Y - b.Y);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit2D operator -(Unit2D u)  => new(-u.X, -u.Y);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit2D operator *(Unit2D u, double scalar) => new(u.X * scalar, u.Y * scalar);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit2D operator /(Unit2D u, double scalar) => new(u.X / scalar, u.Y / scalar);

    public override string ToString() => $"[{X}, {Y}]";
}
