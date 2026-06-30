namespace StencilPad.Spatial;

public static class PolygonUtil
{
    public static bool ContainsPoint(Polygon polygon, Unit2D point, Unit lineWidth)
    {
        if (polygon.Vertices.Count < 2)
        {
            return false;
        }
        
        if (polygon.Vertices.Count == 2)
        {
            var line = new Line(polygon.Vertices[0].Position,
                                polygon.Vertices[1].Position);

            return line.DistanceTo(point) <= Unit.Max(lineWidth / 2, Unit.FromMillimeters(1));
        }

        var walker = new EvenOddWalker(point, lineWidth);

        polygon.Resolver.Walk(walker);

        if (!walker.Hit && !polygon.Closed)
        {
            walker.AddLine(
                polygon.Vertices[polygon.Vertices.Count - 1].Position,
                polygon.Vertices[0].Position);
        }

        return walker.Hit;
    }
}
