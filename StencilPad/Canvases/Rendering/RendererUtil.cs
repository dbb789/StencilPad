using System.Windows;
using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Rendering;

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

        int vertexCount = polygon.Closed ? polygon.Vertices.Count : polygon.Vertices.Count - 1;
        
        for (int i = 0; i < vertexCount; i++)
        {
            AddEdgeToGeometry(ctx, polygon, i, i == 0);
            AddCornerToGeometry(ctx, polygon, i + 1, false);
        }
    }

    public static void AddEdgeToGeometry(StreamGeometryContext ctx,
                                         IPolygon polygon,
                                         int index,
                                         bool initial = true)
    {
        if (initial)
        {
            ctx.BeginFigure(EdgeBegin(polygon, index).Millimeters,
                            isFilled: true,
                            isClosed: false);
        }

        var edge = polygon.Edges.At(index);
        var nextVertex = polygon.Vertices.At(index + 1);
        
        if (edge.Type == EdgeType.Bezier)
        {
            var prevVertex = polygon.Vertices.At(index);
            var c1 = (prevVertex.Position + edge.ControlBeginOffset).Millimeters;
            var c2 = (nextVertex.Position + edge.ControlEndOffset).Millimeters;
            var c3 = EdgeEnd(polygon, index).Millimeters;

            // Seemingly necessary to stop GetFlattenedPathGeometry() from missing
            // the adjoining vertex and creating a skew towards the bezier.
            ctx.LineTo(EdgeBegin(polygon, index).Millimeters, isStroked: true, isSmoothJoin: true);
            ctx.BezierTo(c1, c2, c3, isStroked: true, isSmoothJoin: true);
        }
        else
        {
            ctx.LineTo(EdgeEnd(polygon, index).Millimeters, isStroked: true, isSmoothJoin: true);
        }
    }
    
    public static void AddCornerToGeometry(StreamGeometryContext ctx,
                                           IPolygon polygon,
                                           int index,
                                           bool initial)
    {
        if (initial)
        {
            ctx.BeginFigure(EdgeBegin(polygon, index).Millimeters,
                            isFilled: true,
                            isClosed: false);
        }

        var cornerType = polygon.Vertices.At(index).CornerType;
        var cornerTangent = GetCornerTangent(polygon, index);

        if (cornerTangent <= 0.0001)
        {
            return;
        }
        
        if (cornerType == CornerType.Rounded)
        {
            var angle = CornerAngle(polygon, index);
            var direction = angle > 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;
            var arcRadius = cornerTangent / Math.Tan(Math.Abs(angle) / 2.0);
            
            ctx.ArcTo(point: EdgeBegin(polygon, index).Millimeters,
                      size: new Size(arcRadius, arcRadius),
                      rotationAngle: 0,
                      isLargeArc: false,
                      sweepDirection: direction,
                      isStroked: true,
                      isSmoothJoin: true);
        }
        else if (cornerType == CornerType.Beveled)
        {
            ctx.LineTo(EdgeBegin(polygon, index).Millimeters, isStroked: true, isSmoothJoin: true);
        }
    }

    private static Unit2D EdgeBegin(IPolygon polygon, int index)
    {
        var vertex = polygon.Vertices.At(index);
        var offset = GetCornerTangent(polygon, index);

        if (offset > 0)
        {
            var nextVertex = polygon.Vertices.At(index + 1);
            var direction = (nextVertex.Position - vertex.Position).Normalized;

            return vertex.Position + direction * offset;
        }

        return vertex.Position;
    }

    private static Unit2D EdgeEnd(IPolygon polygon, int index)
    {
        var nextVertex = polygon.Vertices.At(index + 1);
        var offset = GetCornerTangent(polygon, index + 1);

        if (offset > 0)
        {
            var vertex = polygon.Vertices.At(index);
            var direction = (nextVertex.Position - vertex.Position).Normalized;

            return nextVertex.Position - direction * offset;
        }

        return nextVertex.Position;
    }

    private static double GetCornerTangent(IPolygon polygon, int index)
    {
        // Exit early - no need to calculate tangents for non-corner vertices.
        if (polygon.Vertices.At(index).CornerType == CornerType.None)
        {
            return 0;
        }

        var offsetA = GetSingleCornerTangent(polygon, index - 1);
        var offsetB = GetSingleCornerTangent(polygon, index);
        var offsetC = GetSingleCornerTangent(polygon, index + 1);
        var offsetAB = offsetA + offsetB;
        var offsetBC = offsetB + offsetC;
        var edgeAB = EdgeLength(polygon, index - 1).Millimeters;
        var edgeBC = EdgeLength(polygon, index).Millimeters;
        var scaleAB = 1.0;
        var scaleBC = 1.0;

        // Ensure offsetAB and offsetBC are greater than zero to avoid division
        // by zero
        if (offsetAB > 0.0001 && offsetAB > edgeAB)
        {
            scaleAB = edgeAB / offsetAB;
        }

        if (offsetBC > 0.0001 && offsetBC > edgeBC)
        {
            scaleBC = edgeBC / offsetBC;
        }

        return offsetB * Math.Min(scaleAB, scaleBC);
    }
    
    private static double GetSingleCornerTangent(IPolygon polygon, int index)
    {
        var count = polygon.Vertices.Count;

        // A line cannot have corners.
        if (count <= 2)
        {
            return 0;
        }

        // An open line does not have corners at the start and end vertices.
        if (!polygon.Closed)
        {
            var normalizedIndex = ((index % count) + count) % count;

            if (normalizedIndex == 0 || normalizedIndex == count - 1)
            {
                return 0;
            }
        }

        var vertex = polygon.Vertices.At(index);

        // A corner type of None never has a tangent.
        if (vertex.CornerType == CornerType.None)
        {
            return 0;
        }

        double radius = -1;

        if (vertex.CornerSize.IsUnit)
        {
            radius = vertex.CornerSize.Unit.Millimeters;
        }
        else if (vertex.CornerSize.IsProportion)
        {
            var edgeLength = Unit.Min(EdgeLength(polygon, index - 1), EdgeLength(polygon, index));

            radius = (edgeLength * vertex.CornerSize.Proportion).Millimeters;
        }

        // Case of unhandled size type will fall through with a radius of -1 below.
        if (radius <= 0)
        {
            return 0;
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
