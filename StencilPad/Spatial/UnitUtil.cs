namespace StencilPad.Spatial;

public static class UnitUtil
{
    public static string Format(Unit unit, UnitSettings settings)
    {
        var type = GetDefaultUnitType(settings);

        return Format(unit, type);
    }
    
    public static string Format(Unit unit, UnitType type)
    {
        var val = ToType(unit, type);

        return val.ToString(GetFormat(type));
    }
    
    public static string FormatSuffix(Unit unit, UnitSettings settings)
    {
        var type = GetDefaultUnitType(settings);

        return FormatSuffix(unit, type);
    }
    
    public static string FormatSuffix(Unit unit, UnitType type)
    {
        var val = ToType(unit, type);

        return val.ToString(GetFormatSuffix(type));
    }

    public static string FormatScaled(Unit unit, UnitSettings settings)
    {
        var type = GetDefaultUnitType(settings);

        return FormatScaled(unit, type, settings);
    }

    public static string FormatScaled(Unit unit, UnitType type, UnitSettings settings)
    {
        var val = ToTypeScaled(unit, type, settings);

        return val.ToString(GetFormat(type));
    }

    public static string FormatSuffixScaled(Unit unit, UnitSettings settings)
    {
        var type = GetDefaultUnitType(settings);

        return FormatSuffixScaled(unit, type, settings);
    }
    
    public static string FormatSuffixScaled(Unit unit, UnitType type, UnitSettings settings)
    {
        var val = ToTypeScaled(unit, type, settings);

        return val.ToString(GetFormatSuffix(type));
    }

    public static UnitType GetDefaultUnitType(UnitSettings settings)
    {
        return GetDefaultUnitType(settings.System);
    }

    public static UnitType GetDefaultUnitType(UnitSystem unitSystem)
    {
        return unitSystem switch
        {
            UnitSystem.Metric => UnitType.Millimeters,
            UnitSystem.Imperial => UnitType.Inches,
            _ => throw new ArgumentOutOfRangeException(nameof(unitSystem), $"Unsupported unit system: {unitSystem}")
        };
    }

    public static string GetFormat(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Millimeters => "0.###",
            UnitType.Inches => "0.####",
            _ => "0.####"
        };
    }
    
    public static string GetFormatSuffix(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Millimeters => "0.### mm",
            UnitType.Inches => "0.#### in",
            _ => "0.####"
        };
    }

    private static double ToTypeScaled(Unit unit, UnitType type, UnitSettings settings)
    {
        var val = (unit / settings.Ratio).ToType(type);

        // Filters out odd small values, especially anything like negative zero.
        if (Math.Abs(val) < 0.0000001)
        {
            val = 0;
        }

        return val;
    }

    private static double ToType(Unit unit, UnitType type)
    {
        var val = unit.ToType(type);

        // Filters out odd small values, especially anything like negative zero.
        if (Math.Abs(val) < 0.0000001)
        {
            val = 0;
        }

        return val;
    }
}
