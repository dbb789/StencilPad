using System.Windows;
using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class StreamGeometryWalker() : IGeometryWalker
{
    public StreamGeometryContext Context = null!;

    private bool _closed;
    private bool _figureStarted;

    public bool Begin(int segmentCount, bool closed)
    {
        _closed = closed;
        _figureStarted = false;

        return true;
    }

    public bool Line(int segmentIndex, Unit2D from, Unit2D to)
    {
        EnsureFigure(from);
        
        Context.LineTo(to.Millimeters,
                       isStroked: true,
                       isSmoothJoin: true);

        return true;
    }

    public bool Arc(int segmentIndex, Unit2D start, Unit2D mid, Unit2D end)
    {
        EnsureFigure(start);
        
        var offsetA = start - mid;
        var offsetB = end - mid;
        var angle = Unit2D.SignedAngle(offsetA, offsetB);
        var tangent = Unit.Min(offsetA.Magnitude, offsetB.Magnitude) * Math.Tan(Math.Abs(angle) / 2.0);
        var sweepDirection = angle < 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;
        
        Context.ArcTo(point: end.Millimeters,
                      size: new Size(tangent.Millimeters, tangent.Millimeters),
                      rotationAngle: 0,
                      isLargeArc: false,
                      sweepDirection: sweepDirection,
                      isStroked: true,
                      isSmoothJoin: true);

        return true;
    }

    public bool Bezier(int segmentIndex, Unit2D from, Unit2D c1, Unit2D c2, Unit2D to)
    {
        EnsureFigure(from);
        
        Context.BezierTo(c1.Millimeters,
                         c2.Millimeters,
                         to.Millimeters,
                         isStroked: true,
                         isSmoothJoin: true);

        return true;
    }

    private void EnsureFigure(Unit2D from)
    {
        if (_figureStarted)
        {
            return;
        }
        
        Context.BeginFigure(from.Millimeters, isFilled: true, isClosed: _closed);
        _figureStarted = true;
    }
}
