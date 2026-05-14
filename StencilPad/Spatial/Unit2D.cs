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
        
    public Unit2D Normalized
    {
        get
        {
            var magnitude = Magnitude;
            
            if (magnitude == Unit.Zero)
            {
                return Zero;
            }
            
            return new Unit2D(X / magnitude, Y / magnitude);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit2D Abs(Unit2D u)
    {
        return new(Unit.Abs(u.X), Unit.Abs(u.Y));
    }
    
    public static Unit Determinant(Unit2D a, Unit2D b)
    {
        return (a.X * b.Y) - (a.Y * b.X);
    }
    
    public static Unit Dot(Unit2D a, Unit2D b)
    {
        return (a.X * b.X) + (a.Y * b.Y);
    }
        
    public static double SignedAngle(Unit2D a, Unit2D b)
    {
        return Math.Atan2(Determinant(a, b).Millimeters, Dot(a, b).Millimeters);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit2D operator +(Unit2D a, Unit2D b) => new(a.X + b.X, a.Y + b.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit2D operator -(Unit2D a, Unit2D b) => new(a.X - b.X, a.Y - b.Y);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit2D operator -(Unit2D u)  => new(-u.X, -u.Y);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit2D operator *(Unit2D u, Unit scalar) => new(u.X * scalar, u.Y * scalar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit2D operator *(Unit2D u, double scalar) => new(u.X * scalar, u.Y * scalar);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit2D operator /(Unit2D u, Unit scalar) => new(u.X / scalar, u.Y / scalar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit2D operator /(Unit2D u, double scalar) => new(u.X / scalar, u.Y / scalar);

    public override string ToString() => $"({X}, {Y})";
}
