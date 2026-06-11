namespace StencilPad.Spatial;

public readonly record struct Fraction
{
    public static readonly Fraction One = new(1, 1);

    // NOTE: We're storing the numerator and denominator as one less than their
    // actual values to allow for a default value of zero to represent a
    // fraction of 1/1. This will stop any form of division by zero from
    // occurring when the default value is used.
    public int Numerator => _numerator + 1;
    public int Denominator => _denominator + 1;

    private readonly int _numerator;
    private readonly int _denominator;

    public Fraction(int numerator, int denominator)
    {
        if (denominator == 0)
        {
            throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));
        }
        
        _numerator = numerator - 1;
        _denominator = denominator - 1;
    }
}
