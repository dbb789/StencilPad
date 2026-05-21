using System.Runtime.CompilerServices;
using System.Windows;

namespace StencilPad.Spatial;

public readonly record struct UnitBounds
{
    public static readonly UnitBounds Empty = new UnitBounds(Unit2D.Zero, Unit2D.Zero);

    public Rect Millimeters => new Rect(Min.Millimeters, Max.Millimeters);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnitBounds FromCenterSize(Unit2D center, Unit2D size)
    {
        size = Unit2D.Abs(size);
        
        var min = center - (size / 2);
        var max = center + (size / 2);
        
        return new UnitBounds(min, max);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnitBounds FromMinMax(Unit2D min, Unit2D max)
    {
        return new UnitBounds(new Unit2D(Unit.Min(min.X, max.X), Unit.Min(min.Y, max.Y)),
                              new Unit2D(Unit.Max(min.X, max.X), Unit.Max(min.Y, max.Y)));
    }

    // Allow a null value for the first parameter to simplify union operations
    // over a collection of bounds.
    public static UnitBounds Union(UnitBounds? a, UnitBounds b)
    {
        if (a is null)
        {
            return b;
        }
        
        var minA = a.Value.Min;
        var maxA = a.Value.Max;
        var minB = b.Min;
        var maxB = b.Max;

        return FromMinMax(new Unit2D(Unit.Min(minA.X, minB.X),
                                     Unit.Min(minA.Y, minB.Y)),
                          new Unit2D(Unit.Max(maxA.X, maxB.X),
                                     Unit.Max(maxA.Y, maxB.Y)));
    }
    
    public Unit2D Center => (Min + Max) / 2;
    public Unit2D Size => Max - Min;
    public Unit2D Min => _min;
    public Unit2D Max => _max;

    public Unit2D NW => _min;
    public Unit2D NE => new Unit2D(_max.X, _min.Y);
    public Unit2D SW => new Unit2D(_min.X, _max.Y);
    public Unit2D SE => _max;
    
    private readonly Unit2D _min;
    private readonly Unit2D _max;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private UnitBounds(Unit2D min, Unit2D max)
    {
        _min = min;
        _max = max;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Unit2D point)
    {
        return point.X >= _min.X &&
            point.X <= _max.X &&
            point.Y >= _min.Y &&
            point.Y <= _max.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(UnitBounds other)
    {
        var minA = _min;
        var maxA = _max;
        var minB = other._min;
        var maxB = other._max;

        return minB.X >= minA.X &&
               maxB.X <= maxA.X &&
               minB.Y >= minA.Y &&
               maxB.Y <= maxA.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Intersects(UnitBounds other)
    {
        var minA = _min;
        var maxA = _max;
        var minB = other._min;
        var maxB = other._max;

        return minA.X <= maxB.X &&
               maxA.X >= minB.X &&
               minA.Y <= maxB.Y &&
               maxA.Y >= minB.Y;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnitBounds Extend(Unit2D point)
    {
        var min = _min;
        var max = _max;

        return FromMinMax(new Unit2D(Unit.Min(min.X, point.X),
                                     Unit.Min(min.Y, point.Y)),
                          new Unit2D(Unit.Max(max.X, point.X),
                                     Unit.Max(max.Y, point.Y)));
    }

    public UnitBounds ApplyTransform(UnitTransform transform)
    {
        var nw = transform.Apply(NW);
        var ne = transform.Apply(NE);
        var sw = transform.Apply(SW);
        var se = transform.Apply(SE);

        return FromMinMax(new Unit2D(Unit.Min(Unit.Min(nw.X, ne.X), Unit.Min(sw.X, se.X)),
                                     Unit.Min(Unit.Min(nw.Y, ne.Y), Unit.Min(sw.Y, se.Y))),
                          new Unit2D(Unit.Max(Unit.Max(nw.X, ne.X), Unit.Max(sw.X, se.X)),
                                     Unit.Max(Unit.Max(nw.Y, ne.Y), Unit.Max(sw.Y, se.Y))));
    }

    public override string ToString()
    {
        return $"[{_min}, {_max}]";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnitBounds operator +(UnitBounds bounds, Unit2D offset)
    {
        return new UnitBounds(bounds._min + offset, bounds._max + offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnitBounds operator -(UnitBounds bounds, Unit2D offset)
    {
        return new UnitBounds(bounds._min - offset, bounds._max - offset);
    }
}
