namespace StencilPad.Spatial;

public static class UnitUtil
{
    public static string Format(Unit unit, UnitSettings settings)
    {
        var type = GetDefaultUnitType(settings);

        return Format(unit, type, settings);
    }
    
    public static string Format(Unit unit, UnitType type, UnitSettings settings)
    {
        var val = ToType(unit, type, settings);

        return $"{val:0.####}";
    }
    
    public static string FormatSuffix(Unit unit, UnitSettings settings)
    {
        var type = GetDefaultUnitType(settings);

        return FormatSuffix(unit, type, settings);
    }
    
    public static string FormatSuffix(Unit unit, UnitType type, UnitSettings settings)
    {
        var val = ToType(unit, type, settings);
        var suffix = GetSuffix(type);

        return $"{val:0.####} {suffix}";
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

    public static string GetSuffix(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Millimeters => "mm",
            UnitType.Inches => "in",
            _ => ""
        };
    }

    private static double ToType(Unit unit, UnitType type, UnitSettings settings)
    {
        var val = (unit * settings.Ratio).ToType(type);

        // Filters out odd small values, especially anything like negative zero.
        if (Math.Abs(val) < 0.0000001)
        {
            val = 0;
        }

        return val;
    }
}
