namespace StencilPad.Spatial;

// Wraps an IGeometryWalker and only forwards segments within the range
// [startSegment, startFraction] .. [endSegment, endFraction], splitting
// the boundary segments at the given fractions.
public sealed class ClampedGeometryWalker : IGeometryWalker
{
    private readonly IGeometryWalker _inner;
    private readonly int _startSegment;
    private readonly double _startFraction;
    private readonly int _endSegment;
    private readonly double _endFraction;

    public ClampedGeometryWalker(IGeometryWalker inner,
                                 int startSegment,
                                 double startFraction,
                                 int endSegment,
                                 double endFraction)
    {
        _inner = inner;
        _startSegment  = startSegment;
        _startFraction = startFraction;
        _endSegment = endSegment;
        _endFraction = endFraction;
    }

    public bool Begin(int segmentCount, bool closed)
    {
        var clampedSegmentCount = (_endSegment - _startSegment) + 1;
        
        return _inner.Begin(clampedSegmentCount, closed);
    }

    public bool Line(int segmentIndex, Unit2D from, Unit2D to)
    {
        if (segmentIndex < _startSegment || segmentIndex > _endSegment)
        {
            return segmentIndex <= _endSegment;
        }

        if (segmentIndex == _startSegment && segmentIndex == _endSegment)
        {
            from = Unit2D.Lerp(from, to, _startFraction);

            var remapped = _startFraction < 1.0
                ? (_endFraction - _startFraction) / (1.0 - _startFraction)
                : 0.0;

            to = Unit2D.Lerp(from, to, remapped);
        }
        else if (segmentIndex == _startSegment)
        {
            from = Unit2D.Lerp(from, to, _startFraction);
        }
        else if (segmentIndex == _endSegment)
        {
            to = Unit2D.Lerp(from, to, _endFraction);
        }

        return _inner.Line(segmentIndex - _startSegment, from, to);
    }

    public bool Arc(int segmentIndex, Unit2D start, Unit2D mid, Unit2D end)
    {
        if (segmentIndex < _startSegment || segmentIndex > _endSegment)
        {
            return segmentIndex <= _endSegment;
        }

        if (segmentIndex == _startSegment && segmentIndex == _endSegment)
        {
            start = Unit2D.Lerp(start, end, _startFraction);

            var remapped = _startFraction < 1.0
                ? (_endFraction - _startFraction) / (1.0 - _startFraction)
                : 0.0;

            end = Unit2D.Lerp(start, end, remapped);
        }
        else if (segmentIndex == _startSegment)
        {
            start = Unit2D.Lerp(start, end, _startFraction);
        }
        else if (segmentIndex == _endSegment)
        {
            end = Unit2D.Lerp(start, end, _endFraction);
        }

        return _inner.Arc(segmentIndex - _startSegment, start, mid, end);
    }

    public bool Bezier(int segmentIndex, Unit2D from, Unit2D c1, Unit2D c2, Unit2D to)
    {
        if (segmentIndex < _startSegment || segmentIndex > _endSegment)
        {
            return segmentIndex <= _endSegment;
        }

        var bezier = new Bezier2D(from, c1, c2, to);

        if (segmentIndex == _startSegment && segmentIndex == _endSegment)
        {
            bezier = bezier.SplitRight(_startFraction);
            
            var remapped = _startFraction < 1.0
                ? (_endFraction - _startFraction) / (1.0 - _startFraction)
                : 0.0;
            
            bezier = bezier.SplitLeft(remapped);
        }
        else if (segmentIndex == _startSegment)
        {
            bezier = bezier.SplitRight(_startFraction);
        }
        else if (segmentIndex == _endSegment)
        {
            bezier = bezier.SplitLeft(_endFraction);
        }

        return _inner.Bezier(segmentIndex - _startSegment, bezier.P0, bezier.P1, bezier.P2, bezier.P3);
    }
}
