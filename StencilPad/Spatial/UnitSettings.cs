namespace StencilPad.Spatial;

public readonly record struct UnitSettings
{
    public static readonly UnitSettings Default = new(UnitSystem.Metric, Fraction.One);
    
    public UnitSystem System => _system;
    public Fraction Ratio => _ratio;
    
    private readonly UnitSystem _system;
    private readonly Fraction _ratio;

    public UnitSettings(UnitSystem system, Fraction ratio)
    {
        _system = system;
        _ratio = ratio;
    }
}
