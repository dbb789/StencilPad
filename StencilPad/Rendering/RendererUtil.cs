using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public static class RendererUtil
{
    public static void Render(DrawingContext dc,
                              Pen pen,
                              Brush? brush,
                              IPolygon polygon)
    {
        var geometry = BuildGeometry(polygon);

        if (geometry != null)
        {
            dc.DrawGeometry(brush, pen, geometry);
        }
    }

    public static Geometry? BuildGeometry(IPolygon polygon)
    {
        if (polygon.Vertices.Count < 2)
        {
            return null;
        }

        var geometry = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        using (var ctx = geometry.Open())
        {
            AddToGeometry(ctx, polygon);
        }

        geometry.Freeze();

        return geometry;
    }

    public static void AddToGeometry(StreamGeometryContext ctx,
                                     IPolygon polygon)
    {
        if (polygon.Vertices.Count < 2)
        {
            return;
        }

        PolygonUtil.WalkPolygon(polygon, new StreamGeometryWalker(ctx));
    }
}
