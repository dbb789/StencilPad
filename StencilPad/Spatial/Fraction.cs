namespace StencilPad.Spatial;

public readonly record struct Fraction
{
    public static readonly Fraction One = new(1, 1);
    
    public int Numerator => _numerator;
    public int Denominator => _denominator;

    private readonly int _numerator;
    private readonly int _denominator;

    public Fraction(int numerator, int denominator)
    {
        if (denominator == 0)
        {
            throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));
        }
        
        _numerator = numerator;
        _denominator = denominator;
    }
}
