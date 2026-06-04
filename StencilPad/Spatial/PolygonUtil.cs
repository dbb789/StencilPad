namespace StencilPad.Spatial;

public static class PolygonUtil
{
    public static bool ContainsPoint(Polygon polygon, Unit2D point, Unit tolerance)
    {
        if (polygon.Vertices.Count < 2)
        {
            return false;
        }
        
        if (polygon.Vertices.Count == 2)
        {
            var line = new Line(polygon.Vertices[0].Position,
                                polygon.Vertices[1].Position);

            return line.DistanceTo(point) <= Unit.FromMillimeters(1);
        }

        var walker = new EvenOddWalker(point);

        polygon.Resolver.Walk(walker);

        if (!polygon.Closed)
        {
            walker.AddLine(
                polygon.Vertices[polygon.Vertices.Count - 1].Position,
                polygon.Vertices[0].Position);
        }

        return (walker.Count % 2) == 1;
    }
}
