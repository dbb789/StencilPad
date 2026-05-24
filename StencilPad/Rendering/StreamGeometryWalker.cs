using System.Windows;
using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class StreamGeometryWalker() : IGeometryWalker
{
    public StreamGeometryContext Context = null!;

    public void Begin(Unit2D startPoint, bool closed)
    {
        Context.BeginFigure(startPoint.Millimeters, isFilled: true, isClosed: closed);
    }

    public void Line(Unit2D from, Unit2D to)
    {
        Context.LineTo(to.Millimeters, isStroked: true, isSmoothJoin: true);
    }

    public void Arc(Unit2D start, Unit2D mid, Unit2D end)
    {
        var offsetA = start - mid;
        var offsetB = end - mid;
        var angle = Unit2D.SignedAngle(offsetA, offsetB);
        var tangent = Unit.Min(offsetA.Magnitude, offsetB.Magnitude) * Math.Tan(Math.Abs(angle) / 2.0);

        Context.ArcTo(point: end.Millimeters,
                  size: new Size(tangent.Millimeters, tangent.Millimeters),
                  rotationAngle: 0,
                  isLargeArc: false,
                  sweepDirection: angle < 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                  isStroked: true,
                  isSmoothJoin: true);
    }

    
    public void Bezier(Unit2D from, Unit2D c1, Unit2D c2, Unit2D to)
    {
        Context.LineTo(from.Millimeters, isStroked: true, isSmoothJoin: true);
        Context.BezierTo(c1.Millimeters, c2.Millimeters, to.Millimeters, isStroked: true, isSmoothJoin: true);
    }
}
