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
        var val = (unit * settings.Ratio).ToType(type);

        return $"{val:0.###}";
    }
    
    public static string FormatSuffix(Unit unit, UnitSettings settings)
    {
        var type = GetDefaultUnitType(settings);

        return FormatSuffix(unit, type, settings);
    }
    
    public static string FormatSuffix(Unit unit, UnitType type, UnitSettings settings)
    {
        var val = (unit * settings.Ratio).ToType(type);
        var suffix = GetSuffix(type);

        return $"{val:0.###} {suffix}";
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
}
