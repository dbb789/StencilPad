namespace StencilPad.Spatial;

// Wraps an IGeometryWalker and only forwards segments within the range
// [startSegment, startFraction] .. [endSegment, endFraction], splitting
// the boundary segments at the given fractions.
public class ClampedGeometryWalker : IGeometryWalker
{
    private IGeometryWalker _inner;
    private SegmentPoint _startPoint;
    private SegmentPoint _endPoint;

    public ClampedGeometryWalker(IGeometryWalker inner)
    {
        _inner = inner;
        _startPoint = new SegmentPoint(0, 0.0);
        _endPoint = new SegmentPoint(int.MaxValue, 1.0);
    }

    public void SetStartEnd(SegmentPoint? startPoint, SegmentPoint? endPoint)
    {
        _startPoint = startPoint ?? new SegmentPoint(0, 0.0);
        _endPoint = endPoint ?? new SegmentPoint(int.MaxValue, 1.0);
    }
    
    public bool Begin(int segmentCount, bool closed)
    {
        _endPoint = _endPoint with { Index = Math.Min(_endPoint.Index, segmentCount - 1) };

        var clampedSegmentCount = (_endPoint.Index - _startPoint.Index) + 1;
        
        return _inner.Begin(clampedSegmentCount, closed);
    }

    public bool Line(int segmentIndex, Unit2D from, Unit2D to)
    {
        if (segmentIndex < _startPoint.Index || segmentIndex > _endPoint.Index)
        {
            return segmentIndex <= _endPoint.Index;
        }

        if (segmentIndex == _startPoint.Index && segmentIndex == _endPoint.Index)
        {
            from = Unit2D.Lerp(from, to, _startPoint.Fraction);

            var remapped = _startPoint.Fraction < 1.0
                ? (_endPoint.Fraction - _startPoint.Fraction) / (1.0 - _startPoint.Fraction)
                : 0.0;

            to = Unit2D.Lerp(from, to, remapped);
        }
        else if (segmentIndex == _startPoint.Index)
        {
            from = Unit2D.Lerp(from, to, _startPoint.Fraction);
        }
        else if (segmentIndex == _endPoint.Index)
        {
            to = Unit2D.Lerp(from, to, _endPoint.Fraction);
        }

        return _inner.Line(segmentIndex - _startPoint.Index, from, to);
    }

    public bool Arc(int segmentIndex, Arc arc)
    {
        // if (segmentIndex < _startPoint.Index || segmentIndex > _endPoint.Index)
        // {
        //     return segmentIndex <= _endPoint.Index;
        // }

        // if (segmentIndex == _startPoint.Index && segmentIndex == _endPoint.Index)
        // {
        //     start = Unit2D.Lerp(start, end, _startPoint.Fraction);

        //     var remapped = _startPoint.Fraction < 1.0
        //         ? (_endPoint.Fraction - _startPoint.Fraction) / (1.0 - _startPoint.Fraction)
        //         : 0.0;

        //     end = Unit2D.Lerp(start, end, remapped);
        // }
        // else if (segmentIndex == _startPoint.Index)
        // {
        //     start = Unit2D.Lerp(start, end, _startPoint.Fraction);
        // }
        // else if (segmentIndex == _endPoint.Index)
        // {
        //     end = Unit2D.Lerp(start, end, _endPoint.Fraction);
        // }

        return _inner.Arc(segmentIndex - _startPoint.Index, arc);
    }

    public bool Bezier(int segmentIndex, Bezier2D bezier)
    {
        if (segmentIndex < _startPoint.Index || segmentIndex > _endPoint.Index)
        {
            return segmentIndex <= _endPoint.Index;
        }

        if (segmentIndex == _startPoint.Index && segmentIndex == _endPoint.Index)
        {
            bezier = bezier.SplitRight(_startPoint.Fraction);
            
            var remapped = _startPoint.Fraction < 1.0
                ? (_endPoint.Fraction - _startPoint.Fraction) / (1.0 - _startPoint.Fraction)
                : 0.0;
            
            bezier = bezier.SplitLeft(remapped);
        }
        else if (segmentIndex == _startPoint.Index)
        {
            bezier = bezier.SplitRight(_startPoint.Fraction);
        }
        else if (segmentIndex == _endPoint.Index)
        {
            bezier = bezier.SplitLeft(_endPoint.Fraction);
        }

        return _inner.Bezier(segmentIndex - _startPoint.Index, bezier);
    }
}
