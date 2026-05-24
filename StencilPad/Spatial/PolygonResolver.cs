namespace StencilPad.Spatial;

public class PolygonResolver : IPolygonResolver
{
    private IPolygon _polygon;
    private bool _geometryDirty;
    private List<Unit> _cornerTangents;
    private List<Unit> _scaledCornerTangents;
    private List<Unit2D> _edgeBegin;
    private List<Unit2D> _edgeEnd;
    private List<Unit2D> _clippedC1;
    private List<Unit2D> _clippedC2;
    
    public PolygonResolver(IPolygon polygon)
    {
        _polygon = polygon;
        _geometryDirty = true;
        _cornerTangents = new();
        _scaledCornerTangents = new();
        _edgeBegin = new();
        _edgeEnd = new();
        _clippedC1 = new();
        _clippedC2 = new();
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

    public void WalkPolygon(IGeometryWalker walker)
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
        
        int vertexCount = _polygon.Closed ? _polygon.Vertices.Count : _polygon.Vertices.Count - 1;

        for (int i = 0; i < vertexCount; ++i)
        {
            AddEdgeToGeometry(walker, i, i == 0);
            AddCornerToGeometry(walker, i + 1);
        }
    }
    
    public void WalkEdge(IGeometryWalker walker, int edgeIndex)
    {
        if (_polygon is null)
        {
            return;
        }
        
        UpdateGeometry();
        AddEdgeToGeometry(walker, edgeIndex, true);
    }

    private void AddEdgeToGeometry(IGeometryWalker walker,
                                   int index,
                                   bool initial)
    {
        var edgeBegin = EdgeBegin(index);
        
        if (initial)
        {
            walker.Begin(edgeBegin, _polygon.Closed);
        }

        index = NormalizeVertexIndex(index);

        var edge = _polygon.Edges[index];
        
        if (edge.Type == EdgeType.Bezier)
        {
            walker.Bezier(edgeBegin, _clippedC1[index], _clippedC2[index], EdgeEnd(index));
        }
        else
        {
            walker.Line(edgeBegin, EdgeEnd(index));
        }
    }
    
    private void AddCornerToGeometry(IGeometryWalker walker,
                                            int index)
    {
        var edgeBegin = EdgeBegin(index);

        var cornerType = _polygon.Vertices.At(index).CornerType;
        var cornerTangent = _scaledCornerTangents[NormalizeVertexIndex(index)];

        if (cornerTangent <= Unit.Epsilon)
        {
            return;
        }

        var edgeEnd = EdgeEnd(index - 1);
        
        if (cornerType == CornerType.Rounded)
        {
            walker.Arc(edgeEnd,
                       _polygon.Vertices.At(index).Position,
                       edgeBegin);
        }
        else if (cornerType == CornerType.Beveled)
        {
            walker.Line(edgeEnd, edgeBegin);
        }
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
        _clippedC1.Clear();
        _clippedC2.Clear();
        
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

        for (int i = 0; i < _polygon.Vertices.Count; ++i)
        {
            var (c1, c2) = CalculateClippedBezierControls(i);
            _clippedC1.Add(c1);
            _clippedC2.Add(c2);
        }
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
        var vertex    = _polygon.Vertices[index];

        var incomingEdge = _polygon.Edges.At(index - 1);
        Unit2D incomingDir = incomingEdge.Type == EdgeType.Bezier && incomingEdge.ControlEndOffset.SqrMagnitude > 0
            // Tangent at bezier end: P3 - C2 = -ControlEndOffset
            ? -incomingEdge.ControlEndOffset
            : vertex.Position - _polygon.Vertices[prevIndex].Position;

        var nextIndex    = NormalizeVertexIndex(index + 1);
        var outgoingEdge = _polygon.Edges.At(index);
        Unit2D outgoingDir = outgoingEdge.Type == EdgeType.Bezier && outgoingEdge.ControlBeginOffset.SqrMagnitude > 0
            // Tangent at bezier start: C1 - P0 = ControlBeginOffset
            ? outgoingEdge.ControlBeginOffset
            : _polygon.Vertices[nextIndex].Position - vertex.Position;

        return Unit2D.SignedAngle(incomingDir, outgoingDir);
    }

    private (Unit2D c1, Unit2D c2) CalculateClippedBezierControls(int index)
    {
        if (index >= _polygon.Edges.Count)
        {
            return (Unit2D.Zero, Unit2D.Zero);
        }

        var edge = _polygon.Edges[index];

        if (edge.Type != EdgeType.Bezier)
        {
            return (Unit2D.Zero, Unit2D.Zero);
        }

        var p0       = _polygon.Vertices[index].Position;
        var nextIndex = NormalizeVertexIndex(index + 1);
        var p3       = _polygon.Vertices[nextIndex].Position;
        var c1       = p0 + edge.ControlBeginOffset;
        var c2       = p3 + edge.ControlEndOffset;

        var beginArmLength = edge.ControlBeginOffset.Magnitude;
        var endArmLength   = edge.ControlEndOffset.Magnitude;

        // t approximation: distance along control arm / arm length
        double tBegin = beginArmLength > Unit.Epsilon
            ? Math.Clamp(_scaledCornerTangents[index] / beginArmLength, 0.0, 1.0)
            : 0.0;

        double tEnd = endArmLength > Unit.Epsilon
            ? Math.Clamp(1.0 - _scaledCornerTangents[nextIndex] / endArmLength, 0.0, 1.0)
            : 1.0;

        if (tBegin > 0)
        {
            (p0, c1, c2, p3) = SplitBezierRight(p0, c1, c2, p3, tBegin);
            tEnd = tBegin < 1.0 ? (tEnd - tBegin) / (1.0 - tBegin) : 0.0;
        }

        if (tEnd < 1)
        {
            (_, c1, c2, _) = SplitBezierLeft(p0, c1, c2, p3, tEnd);
        }

        return (c1, c2);
    }

    private static (Unit2D p0, Unit2D c1, Unit2D c2, Unit2D p3) SplitBezierRight(
        Unit2D p0, Unit2D c1, Unit2D c2, Unit2D p3, double t)
    {
        var p01   = p0  + (c1  - p0)  * t;
        var p12   = c1  + (c2  - c1)  * t;
        var p23   = c2  + (p3  - c2)  * t;
        var p012  = p01 + (p12 - p01) * t;
        var p123  = p12 + (p23 - p12) * t;
        var p0123 = p012 + (p123 - p012) * t;

        return (p0123, p123, p23, p3);
    }

    private static (Unit2D p0, Unit2D c1, Unit2D c2, Unit2D p3) SplitBezierLeft(
        Unit2D p0, Unit2D c1, Unit2D c2, Unit2D p3, double t)
    {
        var p01   = p0  + (c1  - p0)  * t;
        var p12   = c1  + (c2  - c1)  * t;
        var p23   = c2  + (p3  - c2)  * t;
        var p012  = p01 + (p12 - p01) * t;
        var p123  = p12 + (p23 - p12) * t;
        var p0123 = p012 + (p123 - p012) * t;

        return (p0, p01, p012, p0123);
    }
}
