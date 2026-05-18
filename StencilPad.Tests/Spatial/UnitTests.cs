namespace StencilPad.Tests.Spatial;

using StencilPad.Spatial;

public class UnitTests
{
    [Test]
    public void FromMillimeters_SetsCorrectValue()
    {
        Assert.That(Unit.FromMillimeters(10).Millimeters, Is.EqualTo(10));
        Assert.That(Unit.FromMillimeters(10.5).Millimeters, Is.EqualTo(10.5));
        Assert.That(Unit.FromMillimeters(10.5m).Millimeters, Is.EqualTo(10.5));
    }

    [Test]
    public void FromInches_SetsCorrectValue()
    {
        const double inches = 1.0;
        const double expectedMm = 25.4;
        
        Assert.That(Unit.FromInches(inches).Millimeters, Is.EqualTo(expectedMm));
        Assert.That(Unit.FromInches(1).Millimeters, Is.EqualTo(25.4));
        Assert.That(Unit.FromInches(1m).Millimeters, Is.EqualTo(25.4));
    }

    [Test]
    public void Inches_ReturnsCorrectValue()
    {
        Assert.That(Unit.FromMillimeters(25.4).Inches, Is.EqualTo(1.0).Within(0.0000001));
    }

    [Test]
    public void FromType_CreatesCorrectUnit()
    {
        Assert.That(Unit.FromType(10, UnitType.Millimeters).Millimeters, Is.EqualTo(10));
        Assert.That(Unit.FromType(1, UnitType.Inches).Millimeters, Is.EqualTo(25.4));
    }

    [Test]
    public void ToType_ReturnsCorrectValue()
    {
        var unit = Unit.FromMillimeters(25.4);
        Assert.That(unit.ToType(UnitType.Millimeters), Is.EqualTo(25.4));
        Assert.That(unit.ToType(UnitType.Inches), Is.EqualTo(1.0).Within(0.0000001));
    }

    [Test]
    public void TryParse_ValidString_ReturnsTrueAndCorrectValue()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Unit.TryParse("10.5", out var result1), Is.True);
            Assert.That(result1.Millimeters, Is.EqualTo(10.5));

            Assert.That(Unit.TryParse("1", UnitType.Inches, out var result2), Is.True);
            Assert.That(result2.Millimeters, Is.EqualTo(25.4));
        });
    }

    [Test]
    public void TryParse_InvalidString_ReturnsFalseAndZero()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Unit.TryParse("abc", out var result), Is.False);
            Assert.That(result, Is.EqualTo(Unit.Zero));
        });
    }

    [Test]
    public void Arithmetic_Operators_WorkCorrectly()
    {
        var a = Unit.FromMillimeters(10);
        var b = Unit.FromMillimeters(5);

        Assert.Multiple(() =>
        {
            Assert.That((a + b).Millimeters, Is.EqualTo(15));
            Assert.That((a - b).Millimeters, Is.EqualTo(5));
            Assert.That((-a).Millimeters, Is.EqualTo(-10));
            Assert.That((a * b).Millimeters, Is.EqualTo(50));
            Assert.That((a * 2.0).Millimeters, Is.EqualTo(20));
            Assert.That((a / b).Millimeters, Is.EqualTo(2));
            Assert.That((a / 2.0).Millimeters, Is.EqualTo(5));
        });
    }

    [Test]
    public void Comparison_Operators_WorkCorrectly()
    {
        var a = Unit.FromMillimeters(10);
        var b = Unit.FromMillimeters(5);
        var c = Unit.FromMillimeters(10);

        Assert.Multiple(() =>
        {
            Assert.That(a > b, Is.True);
            Assert.That(b < a, Is.True);
            Assert.That(a >= c, Is.True);
            Assert.That(a <= c, Is.True);
            Assert.That(a >= b, Is.True);
            Assert.That(b <= a, Is.True);
        });
    }

    [Test]
    public void UtilityMethods_WorkCorrectly()
    {
        var a = Unit.FromMillimeters(10);
        var b = Unit.FromMillimeters(-5);

        Assert.Multiple(() =>
        {
            Assert.That(Unit.Abs(b).Millimeters, Is.EqualTo(5));
            Assert.That(Unit.Max(a, b).Millimeters, Is.EqualTo(10));
            Assert.That(Unit.Min(a, b).Millimeters, Is.EqualTo(-5));
            Assert.That(Unit.Clamp(Unit.FromMillimeters(15), b, a).Millimeters, Is.EqualTo(10));
        });
    }

    [Test]
    public void ToString_ReturnsInvariantString()
    {
        Assert.That(Unit.FromMillimeters(10.5).ToString(), Is.EqualTo("10.5"));
    }
}
