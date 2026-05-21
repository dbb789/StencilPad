namespace StencilPad.Tests.Spatial;

using StencilPad.Spatial;

public class UnitTransformTests
{
    private static Unit U(double v) => Unit.FromMillimeters(v);
    private static Unit2D U2(double x, double y) => new(U(x), U(y));

    [Test]
    public void Identity_DoesNotChangePoint()
    {
        var transform = UnitTransform.Identity;
        var point = U2(10, 20);
        
        var result = transform.Apply(point);
        
        Assert.That(result, Is.EqualTo(point));
    }

    [Test]
    public void Apply_TranslationOnly_MovesPoint()
    {
        var position = U2(5, -10);
        var transform = new UnitTransform(position, 0m);
        var point = U2(10, 20);
        
        var result = transform.Apply(point);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.X.Millimeters, Is.EqualTo(15));
            Assert.That(result.Y.Millimeters, Is.EqualTo(10));
        });
    }

    [Test]
    public void Apply_RotationOnly_RotatesPoint()
    {
        var transform = new UnitTransform(Unit2D.Zero, 90m);
        var point = U2(10, 0);
        
        var result = transform.Apply(point);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.X.Millimeters, Is.EqualTo(0).Within(1e-9));
            Assert.That(result.Y.Millimeters, Is.EqualTo(10).Within(1e-9));
        });
    }

    [Test]
    public void Apply_RotationAndTranslation_TransformsCorrectly()
    {
        var position = U2(5, 5);
        var transform = new UnitTransform(position, 90m);
        var point = U2(10, 0);
        
        var result = transform.Apply(point);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.X.Millimeters, Is.EqualTo(5).Within(1e-9));
            Assert.That(result.Y.Millimeters, Is.EqualTo(15).Within(1e-9));
        });
    }

    [Test]
    public void Apply_ZeroPoint_ReturnsPosition()
    {
        var position = U2(100, 200);
        var transform = new UnitTransform(position, 180m);
        
        var result = transform.Apply(Unit2D.Zero);
        
        Assert.That(result, Is.EqualTo(position));
    }

    [Test]
    public void Rotate_OnlyRotatesVector()
    {
        var position = U2(100, 200);
        var transform = new UnitTransform(position, 90m);
        var vector = U2(10, 0);
        
        var result = transform.Rotate(vector);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.X.Millimeters, Is.EqualTo(0).Within(1e-9));
            Assert.That(result.Y.Millimeters, Is.EqualTo(10).Within(1e-9));
        });
    }

    [Test]
    public void InverseApply_ReversesApply()
    {
        var position = U2(5, 10);
        var transform = new UnitTransform(position, 30m);
        var originalPoint = U2(15, 25);
        
        var transformedPoint = transform.Apply(originalPoint);
        var invertedPoint = transform.InverseApply(transformedPoint);
        
        Assert.Multiple(() =>
        {
            Assert.That(invertedPoint.X.Millimeters, Is.EqualTo(originalPoint.X.Millimeters).Within(1e-9));
            Assert.That(invertedPoint.Y.Millimeters, Is.EqualTo(originalPoint.Y.Millimeters).Within(1e-9));
        });
    }

    [Test]
    public void Invert_ReversesTransformation()
    {
        var position = U2(5, 10);
        var transform = new UnitTransform(position, 30m);
        var inverse = transform.Invert();
        var originalPoint = U2(15, 25);
        
        var transformedPoint = transform.Apply(originalPoint);
        var result = inverse.Apply(transformedPoint);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.X.Millimeters, Is.EqualTo(originalPoint.X.Millimeters).Within(1e-9));
            Assert.That(result.Y.Millimeters, Is.EqualTo(originalPoint.Y.Millimeters).Within(1e-9));
        });
    }

    [Test]
    public void Composition_Operator_WorksCorrectly()
    {
        var t1 = new UnitTransform(U2(10, 0), 90m);
        var t2 = new UnitTransform(U2(10, 0), 90m);
        
        var combined = t1 * t2;
        
        // Applying t2 then t1:
        // Point (0,0) -> t2.Apply -> (10,0)
        // (10,0) -> t1.Apply -> Rotate(10,0 by 90) + (10,0) -> (0,10) + (10,0) -> (10,10)
        
        var result = combined.Apply(Unit2D.Zero);
        
        Assert.Multiple(() =>
        {
            Assert.That(combined.Angle, Is.EqualTo(180m));
            Assert.That(result.X.Millimeters, Is.EqualTo(10).Within(1e-9));
            Assert.That(result.Y.Millimeters, Is.EqualTo(10).Within(1e-9));
        });
    }
}
