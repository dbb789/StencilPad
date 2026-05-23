using System.Globalization;
using System.Runtime.CompilerServices;

namespace StencilPad.Spatial;

public readonly record struct Unit
{
    private const decimal InchesToMillimeters = 25.4m;
    
    public static readonly Unit Zero = new(0);
    public static readonly Unit Epsilon = new(0.0000001m);

    public static Unit FromMillimeters(double millimeters)
    {
        return new Unit((decimal)millimeters);
    }
    
    public static Unit FromMillimeters(int millimeters)
    {
        return new Unit((decimal)millimeters);
    }

    public static Unit FromMillimeters(decimal millimeters)
    {
        return new Unit(millimeters);
    }
    
    public static Unit FromInches(double inches)
    {
        return new Unit((decimal)inches * InchesToMillimeters);
    }
    
    public static Unit FromInches(int inches)
    {
        return new Unit((decimal)inches * InchesToMillimeters);
    }

    public static Unit FromInches(decimal inches)
    {
        return new Unit(inches * InchesToMillimeters);
    }

    public static Unit FromType(decimal value, UnitType type)
    {
        return type switch
        {
            UnitType.Millimeters => FromMillimeters(value),
            UnitType.Inches => FromInches(value),
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unsupported unit type: {type}")
        };
    }

    public static bool TryParse(string s, out Unit result)
    {
        return TryParse(s, UnitType.Millimeters, out result);
    }

    public static bool TryParse(string s, UnitType type, out Unit result)
    {
        if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue))
        {
            result = FromType(parsedValue, type);
            return true;
        }

        result = Zero;
        return false;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Unit(decimal value)
    {
        _value = value;
    }

    private readonly decimal _value;
    
    public double Millimeters => (double)_value;
    public double Inches => (double)(_value / InchesToMillimeters);

    public double ToType(UnitType type)
    {
        return type switch
        {
            UnitType.Millimeters => Millimeters,
            UnitType.Inches => Inches,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unsupported unit type: {type}")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit Abs(Unit u) => new(Math.Abs(u._value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit Max(Unit a, Unit b) => new(Math.Max(a._value, b._value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit Min(Unit a, Unit b) => new(Math.Min(a._value, b._value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit Clamp(Unit value, Unit min, Unit max)
        => new(Math.Clamp(value._value, min._value, max._value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Unit a, Unit b) => a._value < b._value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Unit a, Unit b) => a._value > b._value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Unit a, Unit b) => a._value <= b._value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Unit a, Unit b) => a._value >= b._value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit operator +(Unit a, Unit b) => new(a._value + b._value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit operator -(Unit a, Unit b) => new(a._value - b._value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit operator -(Unit u) => new(-u._value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit operator *(Unit u, double scalar) => new(u._value * (decimal)scalar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit operator /(Unit u, double scalar) => new(u._value / (decimal)scalar);

    public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);
}
