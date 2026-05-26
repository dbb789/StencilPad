using StencilPad.Spatial;

public static class BezierUtil
{
    public static Bezier2D FromPolygonEdge(Polygon polygon, Index edgeIndex)
    {
        if (polygon.Edges.Count < 1)
        {
            throw new ArgumentException("Polygon must have at least one edge.");
        }

        var index = edgeIndex.GetOffset(polygon.Edges.Count);
        var edge = polygon.Edges.At(index);

        var p0 = polygon.Vertices.At(index).Position;
        var p3 = polygon.Vertices.At(index + 1).Position;
        var p1 = p0 + edge.ControlBeginOffset;
        var p2 = p3 + edge.ControlEndOffset;
        
        return new Bezier2D(p0, p1, p2, p3);
    }

    public static Bezier2D FromPolygonEdge(Polygon polygon, UnitTransform transform, Index edgeIndex)
    {
        if (polygon.Edges.Count < 1)
        {
            throw new ArgumentException("Polygon must have at least one edge.");
        }

        var index = edgeIndex.GetOffset(polygon.Edges.Count);
        var edge = polygon.Edges.At(index);

        var v0 = polygon.Vertices.At(index).Position;
        var v1 = polygon.Vertices.At(index + 1).Position;
        
        var p0 = transform.Apply(v0);
        var p3 = transform.Apply(v1);
        var p1 = transform.Apply(v0 + edge.ControlBeginOffset);
        var p2 = transform.Apply(v1 + edge.ControlEndOffset);
        
        return new Bezier2D(p0, p1, p2, p3);
    }
}
