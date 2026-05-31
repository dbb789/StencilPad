using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace StencilPad.Spatial;

public readonly record struct UnitTransform(Unit2D Position, decimal Angle = 0m)
{
    public static readonly UnitTransform Identity = new(Unit2D.Zero, 0m);

    public Transform CreateGroupTransform()
    {
        var group = new TransformGroup();
        if (Angle != 0m)
        {
            group.Children.Add(new RotateTransform((double)Angle));
        }
        group.Children.Add(new TranslateTransform(Position.X.Millimeters, Position.Y.Millimeters));
        group.Freeze();
        return group;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Unit2D Apply(Unit2D point)
    {
        if (Angle == 0m)
        {
            return point + Position;
        }

        var angleRadians = (double)Angle * (Math.PI / 180.0);
        var cos = Math.Cos(angleRadians);
        var sin = Math.Sin(angleRadians);

        var x = point.X.Millimeters;
        var y = point.Y.Millimeters;

        var rx = (x * cos) - (y * sin);
        var ry = (x * sin) + (y * cos);

        return new Unit2D(Unit.FromMillimeters(rx), Unit.FromMillimeters(ry)) + Position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Unit2D Rotate(Unit2D vector)
    {
        if (Angle == 0m)
        {
            return vector;
        }

        var angleRadians = (double)Angle * (Math.PI / 180.0);
        var cos = Math.Cos(angleRadians);
        var sin = Math.Sin(angleRadians);

        var x = vector.X.Millimeters;
        var y = vector.Y.Millimeters;

        var rx = (x * cos) - (y * sin);
        var ry = (x * sin) + (y * cos);

        return new Unit2D(Unit.FromMillimeters(rx), Unit.FromMillimeters(ry));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Unit2D InverseApply(Unit2D point)
    {
        var p = point - Position;

        if (Angle == 0m)
        {
            return p;
        }

        var angleRadians = (double)Angle * (Math.PI / 180.0);
        var cos = Math.Cos(angleRadians);
        var sin = Math.Sin(angleRadians);

        var x = p.X.Millimeters;
        var y = p.Y.Millimeters;

        var rx = (x * cos) + (y * sin);
        var ry = -(x * sin) + (y * cos);

        return new Unit2D(Unit.FromMillimeters(rx), Unit.FromMillimeters(ry));
    }

    public UnitTransform Invert()
    {
        if (Angle == 0m)
        {
            return new UnitTransform(-Position, 0m);
        }

        var invAngle = -Angle;
        var angleRadians = (double)invAngle * (Math.PI / 180.0);
        var cos = Math.Cos(angleRadians);
        var sin = Math.Sin(angleRadians);

        var x = -Position.X.Millimeters;
        var y = -Position.Y.Millimeters;

        var rx = (x * cos) - (y * sin);
        var ry = (x * sin) + (y * cos);

        return new UnitTransform(new Unit2D(Unit.FromMillimeters(rx), Unit.FromMillimeters(ry)), invAngle);
    }

    public static UnitTransform operator *(UnitTransform t1, UnitTransform t2)
    {
        return new UnitTransform(t1.Apply(t2.Position), t1.Angle + t2.Angle);
    }
}
