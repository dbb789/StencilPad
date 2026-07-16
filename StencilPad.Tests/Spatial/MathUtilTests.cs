namespace StencilPad.Tests.Spatial;

using System;
using StencilPad.Spatial;

public class MathUtilTests
{
    private static Unit U(double v) => Unit.FromMillimeters(v);
    private static Unit2D U2(double x, double y) => new(U(x), U(y));

    // --- Angle Helpers ---

    [TestCase(0, 0)]
    [TestCase(Math.PI * 2, 0)]
    [TestCase(-Math.PI, Math.PI)]
    [TestCase(Math.PI * 3, Math.PI)]
    [TestCase(-Math.PI * 3, Math.PI)]
    public void NormalizeAngle_ReturnsCorrectValue(double angle, double expected)
    {
        Assert.That(MathUtil.NormalizeAngle(angle), Is.EqualTo(expected).Within(MathUtil.Epsilon));
    }

    [TestCase(0, Math.PI / 2, Math.PI / 2)]
    [TestCase(Math.PI / 2, 0, -Math.PI / 2)]
    [TestCase(Math.PI * 1.5, 0, Math.PI / 2)]
    [TestCase(0, Math.PI * 1.5, -Math.PI / 2)]
    [TestCase(0, Math.PI, Math.PI)]
    public void SignedAngleDifference_ReturnsShortestPath(double a, double b, double expected)
    {
        Assert.That(MathUtil.SignedAngleDifference(a, b), Is.EqualTo(expected).Within(MathUtil.Epsilon));
    }

    [TestCase(0, Math.PI / 2, Math.PI / 2)]
    [TestCase(Math.PI / 2, 0, Math.PI / 2)]
    [TestCase(Math.PI * 1.5, 0, Math.PI / 2)]
    [TestCase(0, Math.PI * 1.5, Math.PI / 2)]
    public void AngleDifference_ReturnsAbsoluteShortestPath(double a, double b, double expected)
    {
        Assert.That(MathUtil.AngleDifference(a, b), Is.EqualTo(expected).Within(MathUtil.Epsilon));
    }

    [TestCase(0, Math.PI, 0.5, Math.PI / 2)]
    [TestCase(Math.PI * 1.5, 0, 0.5, Math.PI * 1.75)]
    [TestCase(0, Math.PI * 1.5, 0.5, Math.PI * 1.75)]
    public void LerpAngle_FollowsShortestPath(double a, double b, double t, double expected)
    {
        Assert.That(MathUtil.NormalizeAngle(MathUtil.LerpAngle(a, b, t)), 
                    Is.EqualTo(MathUtil.NormalizeAngle(expected)).Within(MathUtil.Epsilon));
    }

    [TestCase(0, Math.PI, Math.PI / 2, 0.5)]
    [TestCase(0, Math.PI / 2, Math.PI / 4, 0.5)]
    public void InverseLerpAngle_ReturnsCorrectFraction(double a, double b, double value, double expected)
    {
        Assert.That(MathUtil.InverseLerpAngle(a, b, value), Is.EqualTo(expected).Within(MathUtil.Epsilon));
    }

    // --- SolveQuadratic ---

    [Test]
    public void SolveQuadratic_StandardTwoSolutions()
    {
        var (t0, t1) = MathUtil.SolveQuadratic(1, -5, 6);
        Assert.Multiple(() =>
        {
            Assert.That(t0, Is.EqualTo(2).Within(MathUtil.Epsilon));
            Assert.That(t1, Is.EqualTo(3).Within(MathUtil.Epsilon));
        });
    }

    [Test]
    public void SolveQuadratic_OneSolution()
    {
        var (t0, t1) = MathUtil.SolveQuadratic(1, -4, 4);
        Assert.Multiple(() =>
        {
            Assert.That(t0, Is.EqualTo(2).Within(MathUtil.Epsilon));
            Assert.That(t1, Is.Null);
        });
    }

    [Test]
    public void SolveQuadratic_NoRealSolutions()
    {
        var (t0, t1) = MathUtil.SolveQuadratic(1, 1, 1);
        Assert.Multiple(() =>
        {
            Assert.That(t0, Is.Null);
            Assert.That(t1, Is.Null);
        });
    }

    [Test]
    public void SolveQuadratic_LinearEquation()
    {
        var (t0, t1) = MathUtil.SolveQuadratic(0, 2, -4);
        Assert.Multiple(() =>
        {
            Assert.That(t0, Is.EqualTo(2).Within(MathUtil.Epsilon));
            Assert.That(t1, Is.Null);
        });
    }

    [Test]
    public void SolveQuadratic_ZeroEquation()
    {
        var (t0, t1) = MathUtil.SolveQuadratic(0, 0, 0);
        Assert.Multiple(() =>
        {
            Assert.That(t0, Is.Null);
            Assert.That(t1, Is.Null);
        });
    }

    // --- SolveQuadratic01 ---

    [Test]
    public void SolveQuadratic01_SolutionsOutsideBounds_ClampedOrIgnored()
    {
        var (t0, t1) = MathUtil.SolveQuadratic01(1, -5, 4);
        Assert.Multiple(() =>
        {
            Assert.That(t0, Is.EqualTo(1).Within(MathUtil.Epsilon));
            Assert.That(t1, Is.Null);
        });
    }

    [Test]
    public void SolveQuadratic01_EpsilonClamping()
    {
        var (t0, t1) = MathUtil.SolveQuadratic01(1, -1, -1e-12);
        Assert.Multiple(() =>
        {
            Assert.That(t0, Is.EqualTo(0));
            Assert.That(t1, Is.EqualTo(1));
        });
    }

    // --- Circle-Line Intersection ---

    [Test]
    public void GetCircleLineIntersection_SecantLine_TwoPoints()
    {
        var center = U2(0, 0);
        var radius = U(5);
        var line = new Line(U2(-10, 0), U2(10, 0));

        var (p0, p1) = MathUtil.GetCircleLineIntersection(center, radius, line);

        Assert.Multiple(() =>
        {
            Assert.That(p0, Is.Not.Null);
            Assert.That(p1, Is.Not.Null);
            Assert.That(p0!.Value.X.Millimeters, Is.EqualTo(-5).Within(MathUtil.Epsilon));
            Assert.That(p1!.Value.X.Millimeters, Is.EqualTo(5).Within(MathUtil.Epsilon));
            Assert.That(p0!.Value.Y.Millimeters, Is.EqualTo(0).Within(MathUtil.Epsilon));
            Assert.That(p1!.Value.Y.Millimeters, Is.EqualTo(0).Within(MathUtil.Epsilon));
        });
    }

    [Test]
    public void GetCircleLineIntersection_TangentLine_OnePoint()
    {
        var center = U2(0, 0);
        var radius = U(5);
        var line = new Line(U2(-10, 5), U2(10, 5));

        var (p0, p1) = MathUtil.GetCircleLineIntersection(center, radius, line);

        Assert.Multiple(() =>
        {
            Assert.That(p0, Is.Not.Null);
            Assert.That(p1, Is.Null);
            Assert.That(p0!.Value.X.Millimeters, Is.EqualTo(0).Within(MathUtil.Epsilon));
            Assert.That(p0!.Value.Y.Millimeters, Is.EqualTo(5).Within(MathUtil.Epsilon));
        });
    }

    [Test]
    public void GetCircleLineIntersection_OutsideLine_NoPoints()
    {
        var center = U2(0, 0);
        var radius = U(5);
        var line = new Line(U2(-10, 10), U2(10, 10));

        var (p0, p1) = MathUtil.GetCircleLineIntersection(center, radius, line);

        Assert.Multiple(() =>
        {
            Assert.That(p0, Is.Null);
            Assert.That(p1, Is.Null);
        });
    }

    [Test]
    public void GetCircleLineIntersection_SegmentDoesNotReach_ReturnsNull()
    {
        var center = U2(0, 0);
        var radius = U(5);
        var line = new Line(U2(-10, 0), U2(-6, 0));

        var (p0, p1) = MathUtil.GetCircleLineIntersection(center, radius, line);

        Assert.Multiple(() =>
        {
            Assert.That(p0, Is.Null);
            Assert.That(p1, Is.Null);
        });
    }

    // --- Circle-Circle Intersection ---

    [Test]
    public void GetCircleCircleIntersection_TwoPoints()
    {
        var (p0, p1) = MathUtil.GetCircleCircleIntersection(U2(0, 0), U(5), U2(8, 0), U(5));
        
        Assert.Multiple(() =>
        {
            Assert.That(p0, Is.Not.Null);
            Assert.That(p1, Is.Not.Null);
            Assert.That(p0!.Value.X.Millimeters, Is.EqualTo(4).Within(MathUtil.Epsilon));
            Assert.That(Math.Abs(p0!.Value.Y.Millimeters), Is.EqualTo(3).Within(MathUtil.Epsilon));
            Assert.That(p1!.Value.X.Millimeters, Is.EqualTo(4).Within(MathUtil.Epsilon));
            Assert.That(Math.Abs(p1!.Value.Y.Millimeters), Is.EqualTo(3).Within(MathUtil.Epsilon));
        });
    }

    [Test]
    public void GetCircleCircleIntersection_Tangent()
    {
        var (p0, p1) = MathUtil.GetCircleCircleIntersection(U2(0, 0), U(5), U2(10, 0), U(5));
        
        Assert.Multiple(() =>
        {
            Assert.That(p0, Is.Not.Null);
            Assert.That(p1, Is.Null);
            Assert.That(p0!.Value.X.Millimeters, Is.EqualTo(5).Within(MathUtil.Epsilon));
            Assert.That(p0!.Value.Y.Millimeters, Is.EqualTo(0).Within(MathUtil.Epsilon));
        });
    }

    [Test]
    public void GetCircleCircleIntersection_Disjoint()
    {
        var (p0, p1) = MathUtil.GetCircleCircleIntersection(U2(0, 0), U(5), U2(15, 0), U(5));
        
        Assert.Multiple(() =>
        {
            Assert.That(p0, Is.Null);
            Assert.That(p1, Is.Null);
        });
    }

    [Test]
    public void GetCircleCircleIntersection_Inside()
    {
        var (p0, p1) = MathUtil.GetCircleCircleIntersection(U2(0, 0), U(10), U2(0, 0), U(5));
        
        Assert.Multiple(() =>
        {
            Assert.That(p0, Is.Null);
            Assert.That(p1, Is.Null);
        });
    }

    // --- CircleFromArc ---

    [Test]
    public void CircleFromArc_ValidArc_FindsCenterAndRadius()
    {
        // 5 M----E
        // 4 ------
        // 3 ------
        // 2 ------
        // 1 ------
        // 0 S----C
        //   012345
        var start = U2(0, 0);
        var mid = U2(0, 5);
        var end = U2(5, 5);

        var (center, radius) = MathUtil.CircleFromArc(start, mid, end);

        Assert.Multiple(() =>
        {
            Assert.That(center.X.Millimeters, Is.EqualTo(5).Within(MathUtil.Epsilon));
            Assert.That(center.Y.Millimeters, Is.EqualTo(0).Within(MathUtil.Epsilon));
            Assert.That(radius.Millimeters, Is.EqualTo(5).Within(MathUtil.Epsilon));
        });
    }

    [Test]
    public void CircleFromArc_ColinearPoints_DoesNotThrow()
    {
        var start = U2(-5, 0);
        var mid = U2(0, 0);
        var end = U2(5, 0);

        Assert.That(() => MathUtil.CircleFromArc(start, mid, end), Throws.Nothing);
    }

    // --- RemapPoint ---

    [Test]
    public void RemapPoint_MapsCorrectly()
    {
        var oldBounds = UnitBounds.FromCenterSize(U2(0, 0), U2(100, 100)); // Min -50,-50 Max 50,50
        var newBounds = UnitBounds.FromCenterSize(U2(0, 0), U2(200, 200)); // Min -100,-100 Max 100,100
        var transform = UnitTransform.Identity;

        var result = MathUtil.RemapPoint(U2(50, 50), oldBounds, newBounds, transform);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.X.Millimeters, Is.EqualTo(100).Within(MathUtil.Epsilon));
            Assert.That(result.Y.Millimeters, Is.EqualTo(100).Within(MathUtil.Epsilon));
        });
    }
}
