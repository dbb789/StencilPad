using System.Windows;
using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class StreamGeometryWalker() : IGeometryWalker
{
    public StreamGeometryContext Context = null!;

    public Unit2D StartPosition => _startPosition;
    public Unit2D EndPosition => _endPosition;
    
    private bool _closed;
    private bool _figureStarted;
    private Unit2D _startPosition;
    private Unit2D _endPosition;
    
    public bool Begin(int segmentCount, bool closed)
    {
        _closed = closed;
        _figureStarted = false;

        return true;
    }

    public bool Line(int segmentIndex, Line line)
    {
        EnsureFigure(line.Start, line.End);
        
        Context.LineTo(line.End.Millimeters,
                       isStroked: true,
                       isSmoothJoin: false);

        return true;
    }

    public bool Arc(int segmentIndex, Arc arc)
    {
        var start = arc.Start;
        var end = arc.End;
        
        EnsureFigure(start, end);

        var angle = MathUtil.SignedAngleDifference(arc.EndAngle, arc.StartAngle);
        var radius = arc.Radius.Millimeters;
        var sweepDirection = angle < 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;
                
        Context.ArcTo(point: end.Millimeters,
                      size: new Size(radius, radius),
                      rotationAngle: 0,
                      isLargeArc: false,
                      sweepDirection: sweepDirection,
                      isStroked: true,
                      isSmoothJoin: false);

        return true;
    }
    
    public bool Bezier(int segmentIndex, Bezier2D bezier)
    {
        EnsureFigure(bezier.P0, bezier.P3);
        
        Context.BezierTo(bezier.P1.Millimeters,
                         bezier.P2.Millimeters,
                         bezier.P3.Millimeters,
                         isStroked: true,
                         isSmoothJoin: false);

        return true;
    }

    private void EnsureFigure(Unit2D from, Unit2D to)
    {
        _endPosition = to;
        
        if (_figureStarted)
        {
            return;
        }

        _startPosition = from;
        Context.BeginFigure(from.Millimeters, isFilled: _closed, isClosed: _closed);
        _figureStarted = true;
    }
}
