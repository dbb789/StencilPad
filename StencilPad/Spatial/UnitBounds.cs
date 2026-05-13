using System.Windows;

namespace StencilPad.Spatial;

public readonly record struct UnitBounds
{
    public static readonly UnitBounds Empty = new UnitBounds(Unit2D.Zero, Unit2D.Zero);

    public Rect Millimeters => new Rect(Min.Millimeters, Max.Millimeters);
    
    public static UnitBounds FromCenterSize(Unit2D center, Unit2D size)
    {
        return new UnitBounds(center, Unit2D.Abs(size));
    }

    public static UnitBounds FromMinMax(Unit2D min, Unit2D max)
    {
        var center = (min + max) / 2;
        var size = max - min;
        
        return FromCenterSize(center, size);
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

    private UnitBounds(Unit2D center, Unit2D size)
    {
        Center = center;
        Size = size;
    }

    public Unit2D Center { get; private init; }
    public Unit2D Size { get; private init; }
    public Unit2D Min => Center - Size / 2.0;
    public Unit2D Max => Center + Size / 2.0;

    public bool Contains(Unit2D point)
    {
        var min = Min;
        var max = Max;
        
        return point.X >= min.X &&
            point.X <= max.X &&
            point.Y >= min.Y &&
            point.Y <= max.Y;
    }

    public UnitBounds Extend(Unit2D point)
    {
        var min = Min;
        var max = Max;

        return FromMinMax(new Unit2D(Unit.Min(min.X, point.X),
                                     Unit.Min(min.Y, point.Y)),
                          new Unit2D(Unit.Max(max.X, point.X),
                                     Unit.Max(max.Y, point.Y)));
    }

    public static UnitBounds operator +(UnitBounds bounds, Unit2D offset)
    {
        return new UnitBounds(bounds.Center + offset, bounds.Size);
    }

    public static UnitBounds operator -(UnitBounds bounds, Unit2D offset)
    {
        return new UnitBounds(bounds.Center - offset, bounds.Size);
    }
}
