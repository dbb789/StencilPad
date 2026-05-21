using System.Runtime.CompilerServices;

namespace StencilPad.Spatial;

public readonly record struct UnitTransform(Unit2D Position, decimal Angle)
{
    public static readonly UnitTransform Identity = new(Unit2D.Zero, 0m);

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
}
