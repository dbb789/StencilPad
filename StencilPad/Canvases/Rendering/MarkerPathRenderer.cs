using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Rendering;

public class MarkerPathRenderer : SheetElementRenderer
{
    private record struct MarkerData(Point Position, int SegmentIndex);
    
    public override MarkerPath Element => _markerPath;
    public override UnitBounds SelectionBounds
    {
        get
        {
            if (_geometry is null)
            {
                return UnitBounds.Empty;
            }

            return UnitBounds.FromMinMax(
                new Unit2D(Unit.FromMillimeters(_geometry.Bounds.Left),
                           Unit.FromMillimeters(_geometry.Bounds.Top)),
                new Unit2D(Unit.FromMillimeters(_geometry.Bounds.Right),
                           Unit.FromMillimeters(_geometry.Bounds.Bottom)));
        }
    }

    public int MarkerCount => _markerCount;
    
    private const double MarkerHalfLengthMm = 1.0;

    private MarkerPath _markerPath;
    private StreamGeometry? _geometry;
    private StreamGeometry? _markerGeometry;
    private int _markerCount;
    
    public MarkerPathRenderer(MarkerPath markerPath)
    {
        _markerPath = markerPath;
        _markerPath.EditablePolygon.PolygonChanged += RebuildGeometry;
        _markerPath.PropertyChanged += PropertyChanged;
        _markerCount = 0;
        
        RebuildGeometry();
    }

    public override void Dispose()
    {
        _markerPath.EditablePolygon.PolygonChanged -= RebuildGeometry;
        _markerPath.PropertyChanged -= PropertyChanged;
    }

    public override bool HitTest(Unit2D unit)
    {
        if (_geometry is null)
        {
            return false;
        }

        return _geometry.FillContains(unit.Millimeters);
    }

    public override bool BoundsTest(UnitBounds bounds)
    {
        if (_geometry is null)
        {
            return false;
        }

        var rect = new RectangleGeometry(bounds.Millimeters);

        return _geometry.FillContainsWithDetail(rect) != IntersectionDetail.Empty;
    }

    public override void Render(DrawingContext dc)
    {
        if (_geometry is null)
        {
            return;
        }

        var pen = new Pen(Brushes.Black, 0.2);
        var fill = Brushes.Transparent;

        // if (_markerPath.EditablePolygon.Closed)
        // {
        //     fill = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));
        // }
        
        dc.DrawGeometry(fill, pen, _geometry);

        if (_markerGeometry is not null)
        {
            dc.DrawGeometry(null, new Pen(Brushes.Black, 0.2), _markerGeometry);
        }
    }

    private void PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MarkerPath.Spacing) ||
            e.PropertyName == nameof(MarkerPath.Offset))
        {
            RebuildGeometry();
        }
    }
    
    private void RebuildGeometry()
    {
        _geometry = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        using (var ctx = _geometry.Open())
        {
            RendererUtil.AddToGeometry(ctx, _markerPath.EditablePolygon);
        }

        _geometry.Freeze();

        var points = GetGeometryPoints();

        if (points.Count <= 1)
        {
            _markerGeometry = null;
        }
        else
        {
            _markerGeometry = BuildMarkers(points,
                                           _markerPath.Spacing,
                                           _markerPath.Offset);
        }

        InvokeInvalidateVisual();
    }

    private StreamGeometry? BuildMarkers(List<Point> points, Unit spacing, Unit offset)
    {
        if (spacing.Millimeters < 0.1)
        {
            return null;
        }

        if (points.Count < 2)
        {
            return null;
        }

        var markerData = GenerateMarkerPoints(points, spacing, offset);
        bool balanced = false;
        
        if (_markerPath.EditablePolygon.Closed)
        {
            balanced = BalanceClosingMarker(markerData, points);
        }

        _markerCount = markerData.Count;
        
        var geo = new StreamGeometry { FillRule = FillRule.EvenOdd };
        using var ctx = geo.Open();

        for (int i = 0; i < markerData.Count; i++)
        {
            var marker = markerData[i];
            var perpendicular = GetPerpendicularAt(points, marker.SegmentIndex);
            
            AddLineToContext(ctx, marker.Position, perpendicular, MarkerHalfLengthMm);

            if (i == markerData.Count - 1 && balanced)
            {
                AddCircleToContext(ctx, marker.Position, MarkerHalfLengthMm);
            }
        }

        geo.Freeze();

        return geo;
    }

    private List<MarkerData> GenerateMarkerPoints(List<Point> points, Unit spacing, Unit offset)
    {
        offset = Unit.FromMillimeters(Unit.Abs(offset).Millimeters % spacing.Millimeters);

        var markerData = new List<MarkerData>();

        int pointIndex = 0;
        Point current = points[0];

        while (pointIndex < points.Count - 1)
        {
            var nextStartPoint = GetStartPointOnLine(current,
                                                     offset.Millimeters,
                                                     points[pointIndex],
                                                     points[pointIndex + 1]);

            if (nextStartPoint is not null)
            {
                current = nextStartPoint.Value;
                markerData.Add(new MarkerData(current, pointIndex));
                break;
            }

            ++pointIndex;
        }

        while (pointIndex < points.Count - 1)
        {
            Point? next = GetNextPointAlongLine(current,
                                                spacing.Millimeters,
                                                points[pointIndex],
                                                points[pointIndex + 1]);

            if (next is not null)
            {
                current = next.Value;
                markerData.Add(new MarkerData(current, pointIndex));
            }
            else
            {
                while (++pointIndex < points.Count - 1)
                {
                    var nextStartPoint = GetStartPointOnLine(current,
                                                             spacing.Millimeters,
                                                             points[pointIndex],
                                                             points[pointIndex + 1]);

                    if (nextStartPoint is not null)
                    {
                        current = nextStartPoint.Value;
                        markerData.Add(new MarkerData(current, pointIndex));
                        break;
                    }
                }
            }
        }

        return markerData;
    }

    private bool BalanceClosingMarker(List<MarkerData> markerData, List<Point> points)
    {
        // We need at least 3 markers to balance the last one.
        if (markerData.Count < 3)
        {
            return false;
        }        

        var first = markerData[0];
        var last = markerData[^1];
        var prev = markerData[^2];

        var balanced = FindEquidistantPoint(prev.Position, first.Position, prev.SegmentIndex, points);

        if (balanced.HasValue)
        {
            markerData[^1] = balanced.Value;

            return true;
        }

        // Balance failed - remove last marker.
        markerData.RemoveAt(markerData.Count - 1);
            
        return false;
    }

    // Finds a point P on the path (from segmentIndex onwards) such that |P - a| == |P - b|,
    // i.e. the intersection of the perpendicular bisector of (a, b) with the path.
    private static MarkerData? FindEquidistantPoint(Point a, Point b, int segmentIndex, List<Point> points)
    {
        var mid = new Point((a.X + b.X) / 2.0, (a.Y + b.Y) / 2.0);
        double perpX = -(b.Y - a.Y);
        double perpY =   b.X - a.X;

        for (int idx = segmentIndex; idx < points.Count - 1; idx++)
        {
            var p0 = points[idx];
            var p1 = points[idx + 1];

            double dx = p1.X - p0.X;
            double dy = p1.Y - p0.Y;
            double rx = mid.X - p0.X;
            double ry = mid.Y - p0.Y;

            // Solve: p0 + s*(p1-p0) = mid + t*perp
            // s*dx - t*perpX = rx
            // s*dy - t*perpY = ry
            double det = perpX * dy - perpY * dx;

            if (Math.Abs(det) < 1e-10)
            {
                continue;
            }

            double s = (perpX * ry - perpY * rx) / det;

            if (s >= 0.0 && s <= 1.0)
            {
                return new MarkerData(new Point(p0.X + s * dx, p0.Y + s * dy), idx);
            }
        }

        return null;
    }

    private Point? GetStartPointOnLine(Point center, double radius, Point p0, Point p1)
    {
        var (i0, i1) = GetIntersectionPoints(center, radius, p0, p1);

        // Get the intersection point closest to p0 as the next point.
        if (i0 is not null && i1 is not null)
        {
            return ((i0.Value - p0).LengthSquared < (i1.Value - p0).LengthSquared) ? i0 : i1;
        }

        return i0 ?? i1;
    }
    
    private Point? GetNextPointAlongLine(Point center, double radius, Point p0, Point p1)
    {
        var (i0, i1) = GetIntersectionPoints(center, radius, p0, p1);
        var sqrDistance = (center - p0).LengthSquared;

        // Get the intersection point that is further from p0 as the next point.
        if (i0 is not null && (i0.Value - p0).LengthSquared > sqrDistance)
        {
            return i0;
        }

        if (i1 is not null && (i1.Value - p0).LengthSquared > sqrDistance)
        {
            return i1;
        }

        return null;
    }
    
    private (Point?, Point?) GetIntersectionPoints(Point center, double radius, Point p0, Point p1)
    {
        Point? i0 = null;
        Point? i1 = null;
        
        double dx = p1.X - p0.X;
        double dy = p1.Y - p0.Y;
        double a = dx * dx + dy * dy;
        double b = 2 * (dx * (p0.X - center.X) + dy * (p0.Y - center.Y));
        double c = (p0.X - center.X) * (p0.X - center.X) + (p0.Y - center.Y) * (p0.Y - center.Y) - radius * radius;
        
        double discriminant = b * b - 4 * a * c;
        
        if (discriminant < 0)
        {
            return (null, null);
        }
        
        double sqrtDiscriminant = Math.Sqrt(discriminant);
        double t1 = (-b + sqrtDiscriminant) / (2 * a);
        double t2 = (-b - sqrtDiscriminant) / (2 * a);

        if (t1 >= 0 && t1 <= 1)
        {
            i0 = new Point(p0.X + t1 * dx, p0.Y + t1 * dy);
        }

        if (t2 >= 0 && t2 <= 1)
        {
            i1 = new Point(p0.X + t2 * dx, p0.Y + t2 * dy);
        }
        
        return (i0, i1);
    }

    private static Vector GetPerpendicularAt(List<Point> points, int segmentIndex)
    {
        int idx = segmentIndex < 0 ? points.Count - 2 : Math.Min(segmentIndex, points.Count - 2);
        var p0 = points[idx];
        var p1 = points[idx + 1];
        double dx = p1.X - p0.X;
        double dy = p1.Y - p0.Y;
        double len = Math.Sqrt(dx * dx + dy * dy);

        if (len < 1e-10)
        {
            return new Vector(0, 1);
        }
        
        return new Vector(-dy / len, dx / len);
    }

    private static void AddLineToContext(StreamGeometryContext ctx, Point center, Vector perp, double halfLength)
    {
        var start = new Point(center.X - perp.X * halfLength, center.Y - perp.Y * halfLength);
        var end   = new Point(center.X + perp.X * halfLength, center.Y + perp.Y * halfLength);
        
        ctx.BeginFigure(start, isFilled: false, isClosed: false);
        ctx.LineTo(end, isStroked: true, isSmoothJoin: false);
    }

    private static void AddCircleToContext(StreamGeometryContext ctx, Point center, double radius)
    {
        ctx.BeginFigure(new Point(center.X + radius, center.Y), isFilled: false, isClosed: true);
        ctx.ArcTo(new Point(center.X - radius, center.Y),
                  new Size(radius, radius),
                  rotationAngle: 0,
                  isLargeArc: false,
                  sweepDirection: SweepDirection.Clockwise,
                  isStroked: true,
                  isSmoothJoin: false);
        ctx.ArcTo(new Point(center.X + radius, center.Y),
                  new Size(radius, radius),
                  rotationAngle: 0,
                  isLargeArc: false,
                  sweepDirection: SweepDirection.Clockwise,
                  isStroked: true,
                  isSmoothJoin: false);
    }

    private List<Point> GetGeometryPoints()
    {
        var points = new List<Point>();

        if (_geometry is null)
        {
            return points;
        }
        
        var flattened = _geometry.GetFlattenedPathGeometry(0.001, ToleranceType.Absolute);

        foreach (var figure in flattened.Figures)
        {
            points.Add(figure.StartPoint);

            foreach (var segment in figure.Segments)
            {
                if (segment is PolyLineSegment polyLine)
                {
                    points.AddRange(polyLine.Points);
                }
            }
        }
        
        return points;
    }
}
