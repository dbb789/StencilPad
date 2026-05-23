namespace StencilPad.Spatial;

public class PolygonResolver : IDisposable
{
    private IPolygon _polygon;
    private bool _geometryDirty;
    private List<Unit> _cornerTangents;
    private List<Unit> _scaledCornerTangents;
    private List<Unit2D> _edgeBegin;
    private List<Unit2D> _edgeEnd;
    
    public PolygonResolver()
    {
        _polygon = null!;
        _geometryDirty = false;
        _cornerTangents = new();
        _scaledCornerTangents = new();
        _edgeBegin = new();
        _edgeEnd = new();
    }

    public void Dispose()
    {
        ClearPolygon();
    }

    public void SetPolygon(IPolygon? polygon)
    {
        if (_polygon is not null)
        {
            _polygon.GeometryChanged -= MarkGeometryDirty;
        }
        
        _polygon = polygon!;

        if (_polygon is not null)
        {
            _geometryDirty = true;
            _polygon.GeometryChanged += MarkGeometryDirty;
        }
    }

    public void ClearPolygon()
    {
        SetPolygon(null);
    }

    private void MarkGeometryDirty()
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

    public void WalkPolygon(IPolygonWalker walker)
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
    
    public void WalkEdge(IPolygonWalker walker, int edgeIndex)
    {
        if (_polygon is null)
        {
            return;
        }
        
        UpdateGeometry();
        AddEdgeToGeometry(walker, edgeIndex, true);
    }

    private void AddEdgeToGeometry(IPolygonWalker walker,
                                   int index,
                                   bool initial)
    {
        var edgeBegin = EdgeBegin(index);
        
        if (initial)
        {
            walker.Begin(edgeBegin);
        }

        index = NormalizeVertexIndex(index);
        
        var nextIndex = NormalizeVertexIndex(index + 1);
        
        var edge = _polygon.Edges[index];
        var nextVertex = _polygon.Vertices[nextIndex];
        
        if (edge.Type == EdgeType.Bezier)
        {
            var prevVertex = _polygon.Vertices[index];
            var c1 = (prevVertex.Position + edge.ControlBeginOffset);
            var c2 = (nextVertex.Position + edge.ControlEndOffset);
            var c3 = EdgeEnd(index);

            // Seemingly necessary to stop GetFlattenedPathGeometry() from missing
            // the adjoining vertex and creating a skew towards the bezier.
            walker.Bezier(edgeBegin, c1, c2, c3);
        }
        else
        {
            walker.Line(edgeBegin, EdgeEnd(index));
        }
    }
    
    private void AddCornerToGeometry(IPolygonWalker walker,
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
    }

    private int NormalizeVertexIndex(int index)
    {
        return ((index + _polygon.Vertices.Count) % _polygon.Vertices.Count);
    }


    private Unit2D CalculateEdgeBegin(int index)
    {
        var nextIndex = NormalizeVertexIndex(index + 1);
        var vertex = _polygon.Vertices[index];
        var offset = _scaledCornerTangents[index];

        if (offset > Unit.Zero)
        {
            var nextVertex = _polygon.Vertices[nextIndex];

            return vertex.Position + (nextVertex.Position - vertex.Position).NormalizedTo(offset);
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
        var prevVertex = _polygon.Vertices.At(index - 1);
        var vertex = _polygon.Vertices.At(index);
        var nextVertex = _polygon.Vertices.At(index + 1);
        var edgeA = vertex.Position - prevVertex.Position;
        var edgeB = nextVertex.Position - vertex.Position;

        return Unit2D.SignedAngle(edgeA, edgeB);
    }
}
