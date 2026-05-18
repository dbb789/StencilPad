namespace StencilPad.Tests.Spatial;

using StencilPad.Spatial;

public class Unit2DTests
{
    [Test]
    public void Magnitude_ReturnsCorrectValue()
    {
        var u = new Unit2D(Unit.FromMillimeters(3), Unit.FromMillimeters(4));
        Assert.That(u.Magnitude.Millimeters, Is.EqualTo(5).Within(0.000001));
    }

    [Test]
    public void Normalized_ReturnsCorrectValue()
    {
        var u = new Unit2D(Unit.FromMillimeters(3), Unit.FromMillimeters(4));
        var normalized = u.Normalized;
        Assert.Multiple(() =>
        {
            Assert.That(normalized.X.Millimeters, Is.EqualTo(0.6).Within(0.000001));
            Assert.That(normalized.Y.Millimeters, Is.EqualTo(0.8).Within(0.000001));
            Assert.That(normalized.Magnitude.Millimeters, Is.EqualTo(1.0).Within(0.000001));
        });
    }

    [Test]
    public void Normalized_ZeroVector_ReturnsZero()
    {
        Assert.That(Unit2D.Zero.Normalized, Is.EqualTo(Unit2D.Zero));
    }

    [Test]
    public void Abs_ReturnsAbsoluteValues()
    {
        var u = new Unit2D(Unit.FromMillimeters(-10), Unit.FromMillimeters(5));
        var abs = Unit2D.Abs(u);
        Assert.Multiple(() =>
        {
            Assert.That(abs.X.Millimeters, Is.EqualTo(10));
            Assert.That(abs.Y.Millimeters, Is.EqualTo(5));
        });
    }

    [Test]
    public void DotProduct_IsCorrect()
    {
        var a = new Unit2D(Unit.FromMillimeters(1), Unit.FromMillimeters(2));
        var b = new Unit2D(Unit.FromMillimeters(3), Unit.FromMillimeters(4));
        Assert.That(Unit2D.Dot(a, b).Millimeters, Is.EqualTo(11));
    }

    [Test]
    public void Determinant_IsCorrect()
    {
        var a = new Unit2D(Unit.FromMillimeters(1), Unit.FromMillimeters(2));
        var b = new Unit2D(Unit.FromMillimeters(3), Unit.FromMillimeters(4));
        // (1*4) - (2*3) = 4 - 6 = -2
        Assert.That(Unit2D.Determinant(a, b).Millimeters, Is.EqualTo(-2));
    }

    [Test]
    public void SignedAngle_IsCorrect()
    {
        var a = new Unit2D(Unit.FromMillimeters(1), Unit.FromMillimeters(0));
        var b = new Unit2D(Unit.FromMillimeters(0), Unit.FromMillimeters(1));
        Assert.That(Unit2D.SignedAngle(a, b), Is.EqualTo(Math.PI / 2).Within(0.000001));
    }

    [Test]
    public void Arithmetic_Operators_WorkCorrectly()
    {
        var a = new Unit2D(Unit.FromMillimeters(10), Unit.FromMillimeters(20));
        var b = new Unit2D(Unit.FromMillimeters(5), Unit.FromMillimeters(2));

        Assert.Multiple(() =>
        {
            Assert.That((a + b), Is.EqualTo(new Unit2D(Unit.FromMillimeters(15), Unit.FromMillimeters(22))));
            Assert.That((a - b), Is.EqualTo(new Unit2D(Unit.FromMillimeters(5), Unit.FromMillimeters(18))));
            Assert.That((-a), Is.EqualTo(new Unit2D(Unit.FromMillimeters(-10), Unit.FromMillimeters(-20))));
            Assert.That((a * 2.0).X.Millimeters, Is.EqualTo(20));
            Assert.That((a / 2.0).X.Millimeters, Is.EqualTo(5));
        });
    }

    [Test]
    public void ToString_FormatsCorrectly()
    {
        var u = new Unit2D(Unit.FromMillimeters(1.5), Unit.FromMillimeters(2.5));
        Assert.That(u.ToString(), Is.EqualTo("(1.5, 2.5)"));
    }
}
