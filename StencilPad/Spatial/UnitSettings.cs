namespace StencilPad.Spatial;

public readonly record struct UnitSettings
{
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
