namespace StencilPad.Tests.Spatial;

using StencilPad.Spatial;

public class UnitBoundsTests
{
    [Test]
    public void FromMinMax_NormalizesOrder()
    {
        var p1 = new Unit2D(Unit.FromMillimeters(10), Unit.FromMillimeters(10));
        var p2 = new Unit2D(Unit.FromMillimeters(0), Unit.FromMillimeters(0));
        var bounds = UnitBounds.FromMinMax(p1, p2);

        Assert.Multiple(() =>
        {
            Assert.That(bounds.Min.X.Millimeters, Is.EqualTo(0));
            Assert.That(bounds.Max.X.Millimeters, Is.EqualTo(10));
        });
    }

    [Test]
    public void FromCenterSize_CalculatesCorrectBounds()
    {
        var center = new Unit2D(Unit.FromMillimeters(10), Unit.FromMillimeters(10));
        var size = new Unit2D(Unit.FromMillimeters(4), Unit.FromMillimeters(6));
        var bounds = UnitBounds.FromCenterSize(center, size);

        Assert.Multiple(() =>
        {
            Assert.That(bounds.Min.X.Millimeters, Is.EqualTo(8));
            Assert.That(bounds.Max.X.Millimeters, Is.EqualTo(12));
            Assert.That(bounds.Min.Y.Millimeters, Is.EqualTo(7));
            Assert.That(bounds.Max.Y.Millimeters, Is.EqualTo(13));
            Assert.That(bounds.Center, Is.EqualTo(center));
            Assert.That(bounds.Size, Is.EqualTo(size));
        });
    }

    [Test]
    public void Union_CombinesBoundsCorrectly()
    {
        var b1 = UnitBounds.FromMinMax(new Unit2D(Unit.Zero, Unit.Zero), new Unit2D(Unit.FromMillimeters(10), Unit.FromMillimeters(10)));
        var b2 = UnitBounds.FromMinMax(new Unit2D(Unit.FromMillimeters(5), Unit.FromMillimeters(5)), new Unit2D(Unit.FromMillimeters(15), Unit.FromMillimeters(15)));
        
        var union = UnitBounds.Union(b1, b2);

        Assert.Multiple(() =>
        {
            Assert.That(union.Min, Is.EqualTo(Unit2D.Zero));
            Assert.That(union.Max, Is.EqualTo(new Unit2D(Unit.FromMillimeters(15), Unit.FromMillimeters(15))));
        });
    }

    [Test]
    public void Union_WithNull_ReturnsOther()
    {
        var b = UnitBounds.FromMinMax(Unit2D.Zero, new Unit2D(Unit.FromMillimeters(10), Unit.FromMillimeters(10)));
        Assert.That(UnitBounds.Union(null, b), Is.EqualTo(b));
    }

    [Test]
    public void ContainsPoint_ReturnsCorrectResult()
    {
        var bounds = UnitBounds.FromMinMax(Unit2D.Zero, new Unit2D(Unit.FromMillimeters(10), Unit.FromMillimeters(10)));
        
        Assert.Multiple(() =>
        {
            Assert.That(bounds.Contains(new Unit2D(Unit.FromMillimeters(5), Unit.FromMillimeters(5))), Is.True);
            Assert.That(bounds.Contains(new Unit2D(Unit.FromMillimeters(11), Unit.FromMillimeters(5))), Is.False);
        });
    }

    [Test]
    public void ContainsBounds_ReturnsCorrectResult()
    {
        var outer = UnitBounds.FromMinMax(Unit2D.Zero, new Unit2D(Unit.FromMillimeters(10), Unit.FromMillimeters(10)));
        var inner = UnitBounds.FromMinMax(new Unit2D(Unit.FromMillimeters(2), Unit.FromMillimeters(2)), new Unit2D(Unit.FromMillimeters(8), Unit.FromMillimeters(8)));
        
        Assert.Multiple(() =>
        {
            Assert.That(outer.Contains(inner), Is.True);
            Assert.That(inner.Contains(outer), Is.False);
        });
    }

    [Test]
    public void Intersects_ReturnsCorrectResult()
    {
        var b1 = UnitBounds.FromMinMax(Unit2D.Zero, new Unit2D(Unit.FromMillimeters(10), Unit.FromMillimeters(10)));
        var b2 = UnitBounds.FromMinMax(new Unit2D(Unit.FromMillimeters(5), Unit.FromMillimeters(5)), new Unit2D(Unit.FromMillimeters(15), Unit.FromMillimeters(15)));
        var b3 = UnitBounds.FromMinMax(new Unit2D(Unit.FromMillimeters(11), Unit.FromMillimeters(11)), new Unit2D(Unit.FromMillimeters(20), Unit.FromMillimeters(20)));

        Assert.Multiple(() =>
        {
            Assert.That(b1.Intersects(b2), Is.True);
            Assert.That(b1.Intersects(b3), Is.False);
        });
    }

    [Test]
    public void Extend_ReturnsCorrectResult()
    {
        var bounds = UnitBounds.FromMinMax(Unit2D.Zero, new Unit2D(Unit.FromMillimeters(10), Unit.FromMillimeters(10)));
        var point = new Unit2D(Unit.FromMillimeters(15), Unit.FromMillimeters(-5));
        
        var extended = bounds.Extend(point);

        Assert.Multiple(() =>
        {
            Assert.That(extended.Min, Is.EqualTo(new Unit2D(Unit.Zero, Unit.FromMillimeters(-5))));
            Assert.That(extended.Max, Is.EqualTo(new Unit2D(Unit.FromMillimeters(15), Unit.FromMillimeters(10))));
        });
    }

    [Test]
    public void Offset_Operators_WorkCorrectly()
    {
        var bounds = UnitBounds.FromMinMax(Unit2D.Zero, new Unit2D(Unit.FromMillimeters(10), Unit.FromMillimeters(10)));
        var offset = new Unit2D(Unit.FromMillimeters(5), Unit.FromMillimeters(2));

        var shifted = bounds + offset;

        Assert.Multiple(() =>
        {
            Assert.That(shifted.Min, Is.EqualTo(offset));
            Assert.That(shifted.Max, Is.EqualTo(new Unit2D(Unit.FromMillimeters(15), Unit.FromMillimeters(12))));
        });
    }
}
