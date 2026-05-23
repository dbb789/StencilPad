namespace StencilPad.Spatial;

public static class PolygonUtil
{
    public static void WalkPolygon(IPolygon polygon, IPolygonWalker walker)
    {
        if (polygon.Vertices.Count < 2)
        {
            return;
        }

        int vertexCount = polygon.Closed ? polygon.Vertices.Count : polygon.Vertices.Count - 1;

        for (int i = 0; i < vertexCount; ++i)
        {
            AddEdgeToGeometry(polygon, walker, i, i == 0);
            AddCornerToGeometry(polygon, walker, i + 1);
        }
    }
    
    public static void WalkEdge(IPolygon polygon, IPolygonWalker walker, int edgeIndex)
    {
        AddEdgeToGeometry(polygon, walker, edgeIndex, true);
    }

    private static void AddEdgeToGeometry(IPolygon polygon,
                                          IPolygonWalker walker,
                                          int index,
                                          bool initial)
    {
        var edgeBegin = EdgeBegin(polygon, index);
        
        if (initial)
        {
            walker.Begin(edgeBegin);
        }

        var edge = polygon.Edges.At(index);
        var nextVertex = polygon.Vertices.At(index + 1);
        
        if (edge.Type == EdgeType.Bezier)
        {
            var prevVertex = polygon.Vertices.At(index);
            var c1 = (prevVertex.Position + edge.ControlBeginOffset);
            var c2 = (nextVertex.Position + edge.ControlEndOffset);
            var c3 = EdgeEnd(polygon, index);

            // Seemingly necessary to stop GetFlattenedPathGeometry() from missing
            // the adjoining vertex and creating a skew towards the bezier.
            walker.Bezier(edgeBegin, c1, c2, c3);
        }
        else
        {
            walker.Line(edgeBegin, EdgeEnd(polygon, index));
        }
    }
    
    private static void AddCornerToGeometry(IPolygon polygon,
                                            IPolygonWalker walker,
                                            int index)
    {
        var edgeBegin = EdgeBegin(polygon, index);

        var cornerType = polygon.Vertices.At(index).CornerType;
        var cornerTangent = GetCornerTangent(polygon, index);

        if (cornerTangent <= Unit.Epsilon)
        {
            return;
        }

        var edgeEnd = EdgeEnd(polygon, index - 1);
        
        if (cornerType == CornerType.Rounded)
        {
            walker.Arc(edgeEnd,
                         polygon.Vertices.At(index).Position,
                         edgeBegin);
        }
        else if (cornerType == CornerType.Beveled)
        {
            walker.Line(edgeEnd, edgeBegin);
        }
    }

    private static Unit2D EdgeBegin(IPolygon polygon, int index)
    {
        var vertex = polygon.Vertices.At(index);
        var offset = GetCornerTangent(polygon, index);

        if (offset > Unit.Zero)
        {
            var nextVertex = polygon.Vertices.At(index + 1);

            return vertex.Position + (nextVertex.Position - vertex.Position).NormalizedTo(offset);
        }

        return vertex.Position;
    }

    private static Unit2D EdgeEnd(IPolygon polygon, int index)
    {
        var nextVertex = polygon.Vertices.At(index + 1);
        var offset = GetCornerTangent(polygon, index + 1);

        if (offset > Unit.Zero)
        {
            var vertex = polygon.Vertices.At(index);

            return nextVertex.Position - (nextVertex.Position - vertex.Position).NormalizedTo(offset);
        }

        return nextVertex.Position;
    }

    private static Unit GetCornerTangent(IPolygon polygon, int index)
    {
        // Exit early - no need to calculate tangents for non-corner vertices.
        if (polygon.Vertices.At(index).CornerType == CornerType.None)
        {
            return Unit.Zero;
        }

        var offsetA = GetSingleCornerTangent(polygon, index - 1);
        var offsetB = GetSingleCornerTangent(polygon, index);
        var offsetC = GetSingleCornerTangent(polygon, index + 1);
        var offsetAB = offsetA + offsetB;
        var offsetBC = offsetB + offsetC;
        var edgeAB = EdgeLength(polygon, index - 1);
        var edgeBC = EdgeLength(polygon, index);
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
    
    private static Unit GetSingleCornerTangent(IPolygon polygon, int index)
    {
        var count = polygon.Vertices.Count;

        // A line cannot have corners.
        if (count <= 2)
        {
            return Unit.Zero;
        }

        // An open line does not have corners at the start and end vertices.
        if (!polygon.Closed)
        {
            var normalizedIndex = ((index % count) + count) % count;

            if (normalizedIndex == 0 || normalizedIndex == count - 1)
            {
                return Unit.Zero;
            }
        }

        var vertex = polygon.Vertices.At(index);

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
            var edgeLength = Unit.Min(EdgeLength(polygon, index - 1), EdgeLength(polygon, index));

            radius = edgeLength * vertex.CornerSize.Proportion;
        }

        // Case of unhandled size type will fall through with a radius of -1 below.
        if (radius <= Unit.Zero)
        {
            return Unit.Zero;
        }

        return radius * Math.Tan(Math.Abs(CornerAngle(polygon, index)) / 2.0);
    }

    private static Unit EdgeLength(IPolygon polygon, int index)
    {
        return (polygon.Vertices.At(index + 1).Position - polygon.Vertices.At(index).Position).Magnitude;
    }
    
    private static double CornerAngle(IPolygon polygon, int index)
    {
        var prevVertex = polygon.Vertices.At(index - 1);
        var vertex = polygon.Vertices.At(index);
        var nextVertex = polygon.Vertices.At(index + 1);
        var edgeA = vertex.Position - prevVertex.Position;
        var edgeB = nextVertex.Position - vertex.Position;

        return Unit2D.SignedAngle(edgeA, edgeB);
    }
}
