namespace StencilPad.Spatial;

public class PolygonResolver : IGeometryResolver
{
    private IPolygon _polygon;
    private bool _geometryDirty;
    private List<Unit> _cornerTangents;
    private List<Unit> _scaledCornerTangents;
    private List<Unit2D> _edgeBegin;
    private List<Unit2D> _edgeEnd;
    private List<Bezier2D?> _clippedBeziers;
    private int _edgeCount;
    private int _cornerCount;
    
    public PolygonResolver(IPolygon polygon)
    {
        _polygon = polygon;
        _geometryDirty = true;
        _cornerTangents = new();
        _scaledCornerTangents = new();
        _edgeBegin = new();
        _edgeEnd = new();
        _clippedBeziers = new();
        _edgeCount = 0;
        _cornerCount = 0;
    }

    public void MarkGeometryDirty()
    {
        _geometryDirty = true;
    }
    
    private void UpdateGeometry()
    {
        if (_geometryDirty)
        {
            PrecalculateEdges();
            _geometryDirty = false;
        }
    }

    public void Walk(IGeometryWalker walker)
    {
        if (_polygon is null)
        {
            return;
        }
        
        if (_polygon.Vertices.Count < 2)
        {
            return;
        }

        UpdateGeometry();
        
        var segmentCount = _edgeCount + _cornerCount;

        if (!walker.Begin(segmentCount, _polygon.Closed))
        {
            return;
        }   

        int segmentIndex = 0;

        for (int i = 0; i < _edgeCount - 1; ++i)
        {
            if (!AddEdgeToGeometry(walker, i, ref segmentIndex))
            {
                return;
            }

            if (!AddCornerToGeometry(walker, i + 1, ref segmentIndex))
            {
                return;
            }   
        }
        
        if (!AddEdgeToGeometry(walker, _edgeCount - 1, ref segmentIndex))
        {
            return;
        }
        
        if (_polygon.Closed)
        {
            AddCornerToGeometry(walker, _edgeCount, ref segmentIndex);
        }
    }
    
    public void WalkReverse(IGeometryWalker walker)
    {
        if (_polygon is null)
        {
            return;
        }

        if (_polygon.Vertices.Count < 2)
        {
            return;
        }

        UpdateGeometry();

        var segmentCount = _edgeCount + _cornerCount;
        
        if (!walker.Begin(segmentCount, _polygon.Closed))
        {
            return;
        }

        int segmentIndex = segmentCount - 1;

        if (_polygon.Closed)
        {
            if (!AddCornerToGeometryReverse(walker, 0, ref segmentIndex))
            {
                return;
            }
        }

        for (int i = _edgeCount - 1; i >= 1; --i)
        {
            if (!AddEdgeToGeometryReverse(walker, i, ref segmentIndex))
            {
                return;
            }

            if (!AddCornerToGeometryReverse(walker, i, ref segmentIndex))
            {
                return;
            }
        }

        AddEdgeToGeometryReverse(walker, 0, ref segmentIndex);
    }

    public void WalkEdge(IGeometryWalker walker, int edgeIndex)
    {
        if (_polygon is null)
        {
            return;
        }
        
        UpdateGeometry();
        
        walker.Begin(1, false);
        
        int segmentIndex = 0;
        
        AddEdgeToGeometry(walker, edgeIndex, ref segmentIndex);
    }

    private bool AddEdgeToGeometry(IGeometryWalker walker,
                                   int index,
                                   ref int segmentIndex)
    {
        bool next = true;
        var edgeBegin = EdgeBegin(index);

        index = NormalizeVertexIndex(index);

        var edge = _polygon.Edges[index];
        
        if (edge.Type == EdgeType.Bezier)
        {
            var bezier = _clippedBeziers[index]!.Value;
            next = walker.Segment(segmentIndex,
                                  PolygonSegment.FromBezier(new Bezier2D(edgeBegin,
                                                                         bezier.P1,
                                                                         bezier.P2,
                                                                         EdgeEnd(index))));
            ++segmentIndex;
        }
        else
        {
            next = walker.Segment(segmentIndex,
                                  PolygonSegment.FromLine(new Line(edgeBegin, EdgeEnd(index))));
            ++segmentIndex;
        }

        return next;
    }

    private bool AddCornerToGeometry(IGeometryWalker walker,
                                     int index,
                                     ref int segmentIndex)
    {
        bool next = true;
        var edgeBegin = EdgeBegin(index);

        var cornerType = _polygon.Vertices.At(index).CornerType;
        var cornerTangent = _scaledCornerTangents[NormalizeVertexIndex(index)];

        if (cornerTangent <= Unit.Epsilon)
        {
            return next;
        }

        var edgeEnd = EdgeEnd(index - 1);
        
        if (cornerType == CornerType.Rounded)
        {
            next = walker.Segment(segmentIndex,
                                  PolygonSegment.FromArc(new Arc(edgeEnd,
                                                                 _polygon.Vertices.At(index).Position,
                                                                 edgeBegin)));
            ++segmentIndex;
        }
        else if (cornerType == CornerType.Beveled)
        {
            next = walker.Segment(segmentIndex,
                                  PolygonSegment.FromLine(new Line(edgeEnd, edgeBegin)));
            ++segmentIndex;
        }

        return next;
    }
    
    private bool AddEdgeToGeometryReverse(IGeometryWalker walker,
                                          int index,
                                          ref int segmentIndex)
    {
        bool next = true;
        var edgeBegin = EdgeBegin(index);

        index = NormalizeVertexIndex(index);

        var edge = _polygon.Edges[index];
        
        if (edge.Type == EdgeType.Bezier)
        {
            var bezier = _clippedBeziers[index]!.Value;
            next = walker.Segment(segmentIndex,
                                  PolygonSegment.FromBezier(new Bezier2D(EdgeEnd(index),
                                                                         bezier.P2,
                                                                         bezier.P1,
                                                                         edgeBegin)));
            --segmentIndex;
        }
        else
        {
            next = walker.Segment(segmentIndex,
                                  PolygonSegment.FromLine(new Line(EdgeEnd(index), edgeBegin)));
            --segmentIndex;
        }

        return next;
    }

    private bool AddCornerToGeometryReverse(IGeometryWalker walker,
                                            int index,
                                            ref int segmentIndex)
    {
        bool next = true;
        var edgeBegin = EdgeBegin(index);

        var cornerType = _polygon.Vertices.At(index).CornerType;
        var cornerTangent = _scaledCornerTangents[NormalizeVertexIndex(index)];

        if (cornerTangent <= Unit.Epsilon)
        {
            return next;
        }

        var edgeEnd = EdgeEnd(index - 1);
        
        if (cornerType == CornerType.Rounded)
        {
            next = walker.Segment(segmentIndex,
                                  PolygonSegment.FromArc(new Arc(edgeBegin,
                                                                 _polygon.Vertices.At(index).Position,
                                                                 edgeEnd)));
            --segmentIndex;
        }
        else if (cornerType == CornerType.Beveled)
        {
            next = walker.Segment(segmentIndex,
                                  PolygonSegment.FromLine(new Line(edgeBegin, edgeEnd)));
            --segmentIndex;
        }

        return next;
    }

    private Unit2D EdgeBegin(int index)
    {
        return _edgeBegin[NormalizeVertexIndex(index)];
    }

    private Unit2D EdgeEnd(int index)
    {
        return _edgeEnd[NormalizeVertexIndex(index)];
    }

    ////////////////////////////////////////
    // Edge start/end precalculations
    ////////////////////////////////////////
    
    private void PrecalculateEdges()
    {
        _scaledCornerTangents.Clear();
        _cornerTangents.Clear();
        _edgeBegin.Clear();
        _edgeEnd.Clear();
        _clippedBeziers.Clear();
        
        if (_polygon is null)
        {
            return;
        }

        for (int i = 0; i < _polygon.Vertices.Count; ++i)
        {
            _cornerTangents.Add(CalculateSingleCornerTangent(i));
        }
        
        for (int i = 0; i < _polygon.Vertices.Count; ++i)
        {
            _scaledCornerTangents.Add(CalculateScaledCornerTangent(i));
        }

        for (int i = 0; i < _polygon.Vertices.Count; ++i)
        {
            _edgeBegin.Add(CalculateEdgeBegin(i));
            _edgeEnd.Add(CalculateEdgeEnd(i));
        }

        _cornerCount = 0;

        for (int i = 0; i < _polygon.Vertices.Count; ++i)
        {
            _clippedBeziers.Add(CalculateClippedBezier(i));
        }

        for (int i = 0; i < _polygon.Vertices.Count - 1; ++i)
        {
            if (HasCorner(i))
            {
                ++_cornerCount;
            }
        }

        if (_polygon.Closed && HasCorner(_polygon.Vertices.Count - 1))
        {
            ++_cornerCount;
        }
        
        _edgeCount = _polygon.Closed ? _polygon.Vertices.Count : _polygon.Vertices.Count - 1;
    }

    private bool HasCorner(int index)
    {
        return _polygon.Vertices.At(index).CornerType != CornerType.None &&
                _scaledCornerTangents[NormalizeVertexIndex(index)] > Unit.Epsilon;
    }

    private int NormalizeVertexIndex(int index)
    {
        return ((index + _polygon.Vertices.Count) % _polygon.Vertices.Count);
    }

    private Unit2D CalculateEdgeBegin(int index)
    {
        var vertex = _polygon.Vertices[index];
        var offset = _scaledCornerTangents[index];

        if (offset > Unit.Zero)
        {
            // Use the control arm direction for bezier edges so the arc joining this
            // corner is tangent to the curve rather than to the chord.
            if (index < _polygon.Edges.Count)
            {
                var edge = _polygon.Edges[index];

                if (edge.Type == EdgeType.Bezier && edge.ControlBeginOffset.SqrMagnitude > 0)
                {
                    return vertex.Position + edge.ControlBeginOffset.NormalizedTo(offset);
                }
            }

            var nextIndex = NormalizeVertexIndex(index + 1);

            return vertex.Position + (_polygon.Vertices[nextIndex].Position - vertex.Position).NormalizedTo(offset);
        }

        return vertex.Position;
    }

    private Unit2D CalculateEdgeEnd(int index)
    {
        var nextIndex = NormalizeVertexIndex(index + 1);
        var nextVertex = _polygon.Vertices[nextIndex];
        var offset = _scaledCornerTangents[nextIndex];

        if (offset > Unit.Zero)
        {
            // Use the control arm direction for bezier edges so the arc joining the
            // next corner is tangent to the curve rather than to the chord.
            if (index < _polygon.Edges.Count)
            {
                var edge = _polygon.Edges[index];

                if (edge.Type == EdgeType.Bezier && edge.ControlEndOffset.SqrMagnitude > 0)
                {
                    // ControlEndOffset points from nextVertex toward C2, which is
                    // backward along the curve — exactly the direction we want.
                    return nextVertex.Position + edge.ControlEndOffset.NormalizedTo(offset);
                }
            }

            var vertex = _polygon.Vertices[index];

            return nextVertex.Position - (nextVertex.Position - vertex.Position).NormalizedTo(offset);
        }

        return nextVertex.Position;
    }

    private Unit CalculateScaledCornerTangent(int index)
    {
        // Exit early - no need to calculate tangents for non-corner vertices.
        if (_polygon.Vertices[index].CornerType == CornerType.None)
        {
            return Unit.Zero;
        }

        var prevIndex = NormalizeVertexIndex(index - 1);
        var nextIndex = NormalizeVertexIndex(index + 1);
        var offsetA = _cornerTangents[prevIndex];
        var offsetB = _cornerTangents[index];
        var offsetC = _cornerTangents[nextIndex];
        var offsetAB = offsetA + offsetB;
        var offsetBC = offsetB + offsetC;
        var edgeAB = EdgeLength(prevIndex);
        var edgeBC = EdgeLength(index);
        var scaleAB = 1.0;
        var scaleBC = 1.0;

        // Ensure offsetAB and offsetBC are greater than zero to avoid division
        // by zero
        if (offsetAB > Unit.Epsilon && offsetAB > edgeAB)
        {
            scaleAB = edgeAB / offsetAB;
        }

        if (offsetBC > Unit.Epsilon && offsetBC > edgeBC)
        {
            scaleBC = edgeBC / offsetBC;
        }

        return offsetB * Math.Min(scaleAB, scaleBC);
    }
    
    private Unit CalculateSingleCornerTangent(int index)
    {
        var count = _polygon.Vertices.Count;

        // A line cannot have corners.
        if (count <= 2)
        {
            return Unit.Zero;
        }

        // An open line does not have corners at the start and end vertices.
        if (!_polygon.Closed)
        {
            if (index == 0 || index == count - 1)
            {
                return Unit.Zero;
            }
        }

        var vertex = _polygon.Vertices[index];

        // A corner type of None never has a tangent.
        if (vertex.CornerType == CornerType.None)
        {
            return Unit.Zero;
        }

        Unit radius = Unit.Zero;

        if (vertex.CornerSize.IsUnit)
        {
            radius = vertex.CornerSize.Unit;
        }
        else if (vertex.CornerSize.IsProportion)
        {
            var edgeLength = Unit.Min(EdgeLength(index - 1), EdgeLength(index));

            radius = edgeLength * vertex.CornerSize.Proportion;
        }

        // Case of unhandled size type will fall through with a radius of -1 below.
        if (radius <= Unit.Zero)
        {
            return Unit.Zero;
        }

        return radius * Math.Tan(Math.Abs(CornerAngle(index)) / 2.0);
    }

    private Unit EdgeLength(int index)
    {
        return (_polygon.Vertices.At(index + 1).Position - _polygon.Vertices.At(index).Position).Magnitude;
    }
    
    private double CornerAngle(int index)
    {
        var prevIndex = NormalizeVertexIndex(index - 1);
        var vertex = _polygon.Vertices[index];

        var incomingEdge = _polygon.Edges.At(index - 1);
        Unit2D incomingDir = incomingEdge.Type == EdgeType.Bezier && incomingEdge.ControlEndOffset.SqrMagnitude > 0
            // Tangent at bezier end: P3 - C2 = -ControlEndOffset
            ? -incomingEdge.ControlEndOffset
            : vertex.Position - _polygon.Vertices[prevIndex].Position;

        var nextIndex = NormalizeVertexIndex(index + 1);
        var outgoingEdge = _polygon.Edges.At(index);
        Unit2D outgoingDir = outgoingEdge.Type == EdgeType.Bezier && outgoingEdge.ControlBeginOffset.SqrMagnitude > 0
            // Tangent at bezier start: C1 - P0 = ControlBeginOffset
            ? outgoingEdge.ControlBeginOffset
            : _polygon.Vertices[nextIndex].Position - vertex.Position;

        return Unit2D.SignedAngle(incomingDir, outgoingDir);
    }

    private Bezier2D? CalculateClippedBezier(int index)
    {
        if (index >= _polygon.Edges.Count)
        {
            return null;
        }

        var edge = _polygon.Edges[index];

        if (edge.Type != EdgeType.Bezier)
        {
            return null;
        }

        var p0 = _polygon.Vertices[index].Position;
        var nextIndex = NormalizeVertexIndex(index + 1);
        var p3 = _polygon.Vertices[nextIndex].Position;
        var c1 = p0 + edge.ControlBeginOffset;
        var c2 = p3 + edge.ControlEndOffset;

        var beginArmLength = edge.ControlBeginOffset.Magnitude;
        var endArmLength   = edge.ControlEndOffset.Magnitude;

        double tBegin = beginArmLength > Unit.Epsilon
            ? Math.Clamp(_scaledCornerTangents[index] / beginArmLength, 0.0, 1.0)
            : 0.0;

        double tEnd = endArmLength > Unit.Epsilon
            ? Math.Clamp(1.0 - _scaledCornerTangents[nextIndex] / endArmLength, 0.0, 1.0)
            : 1.0;

        return new Bezier2D(p0, c1, c2, p3).Subsegment(tBegin, tEnd);
    }
}
