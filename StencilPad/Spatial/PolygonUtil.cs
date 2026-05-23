namespace StencilPad.Spatial;

public static class PolygonUtil
{
    public static bool ContainsPoint(Polygon polygon, Unit2D point, Unit tolerance)
    {
        if (polygon.Vertices.Count < 2)
            return false;

        if (polygon.Vertices.Count == 2)
        {
            return IsNearSegment(
                polygon.Vertices[0].Position,
                polygon.Vertices[1].Position,
                point,
                tolerance.Millimeters);
        }

        var walker = new WindingWalker(point);

        polygon.Resolver.WalkPolygon(walker);

        if (!polygon.Closed)
        {
            walker.AddLine(
                polygon.Vertices[polygon.Vertices.Count - 1].Position,
                polygon.Vertices[0].Position);
        }

        return walker.Winding != 0;
    }

    private static bool IsNearSegment(Unit2D a, Unit2D b, Unit2D p, double toleranceMm)
    {
        double ax = a.X.Millimeters, ay = a.Y.Millimeters;
        double bx = b.X.Millimeters, by = b.Y.Millimeters;
        double px = p.X.Millimeters, py = p.Y.Millimeters;

        double dx = bx - ax, dy = by - ay;
        double lenSq = dx * dx + dy * dy;

        double t = lenSq > 1e-20
            ? Math.Clamp(((px - ax) * dx + (py - ay) * dy) / lenSq, 0.0, 1.0)
            : 0.0;

        double cx = ax + t * dx, cy = ay + t * dy;
        double distSq = (px - cx) * (px - cx) + (py - cy) * (py - cy);

        return distSq <= toleranceMm * toleranceMm;
    }

    private static int WindingSegment(Unit2D a, Unit2D b, Unit2D p)
    {
        double ay = a.Y.Millimeters, by = b.Y.Millimeters, py = p.Y.Millimeters;

        double cross = (b.X.Millimeters - a.X.Millimeters) * (py - ay)
                     - (by - ay) * (p.X.Millimeters - a.X.Millimeters);

        if (ay <= py)
        {
            if (by > py && cross > 0) return 1;
        }
        else
        {
            if (by <= py && cross < 0) return -1;
        }

        return 0;
    }

    // Arc(start, mid, end) where mid is the corner vertex (not on arc).
    // Finds the circle from tangent perpendiculars at start and end, then
    // counts winding contributions from horizontal ray crossings within
    // the arc's angular span.
    private static int WindingArc(Unit2D start, Unit2D mid, Unit2D end, Unit2D point)
    {
        double sx = start.X.Millimeters, sy = start.Y.Millimeters;
        double mx = mid.X.Millimeters,   my = mid.Y.Millimeters;
        double ex = end.X.Millimeters,   ey = end.Y.Millimeters;

        // Tangent direction at start = normalize(start - mid) (pointing away from corner).
        // The radius at start is perpendicular to this tangent.
        double t1x = sx - mx, t1y = sy - my;
        double t1len = Math.Sqrt(t1x * t1x + t1y * t1y);
        if (t1len < 1e-10) return 0;
        t1x /= t1len; t1y /= t1len;

        // Tangent direction at end = normalize(end - mid).
        double t2x = ex - mx, t2y = ey - my;
        double t2len = Math.Sqrt(t2x * t2x + t2y * t2y);
        if (t2len < 1e-10) return 0;
        t2x /= t2len; t2y /= t2len;

        // Perpendicular normals (rotate 90° CCW): n = (-ty, tx).
        double n1x = -t1y, n1y = t1x;
        double n2x = -t2y, n2y = t2x;

        // Intersect lines: start + t*n1 = end + s*n2
        // t*n1x - s*n2x = ex - sx
        // t*n1y - s*n2y = ey - sy
        double rx = ex - sx, ry = ey - sy;
        double det = n1x * (-n2y) - n1y * (-n2x);
        if (Math.Abs(det) < 1e-10) return 0;

        double tParam = (rx * (-n2y) - ry * (-n2x)) / det;
        double cx = sx + tParam * n1x;
        double cy = sy + tParam * n1y;

        double r = Math.Sqrt((cx - sx) * (cx - sx) + (cy - sy) * (cy - sy));
        if (r < 1e-10) return 0;

        double py = point.Y.Millimeters;
        double dy = py - cy;
        if (Math.Abs(dy) > r) return 0;

        double px = point.X.Millimeters;
        double halfChord = Math.Sqrt(r * r - dy * dy);

        double startAngle = Math.Atan2(sy - cy, sx - cx);
        double endAngle   = Math.Atan2(ey - cy, ex - cx);

        // Determine arc direction from cross product of (start-center) x (end-center).
        double arcCross = (sx - cx) * (ey - cy) - (sy - cy) * (ex - cx);
        bool ccw = arcCross > 0;

        int winding = 0;

        // Right crossing: (cx + halfChord, py)
        if (cx + halfChord > px)
        {
            double angle = Math.Atan2(dy, halfChord);
            if (IsAngleInArc(angle, startAngle, endAngle, ccw))
                winding += ccw ? 1 : -1;
        }

        // Left crossing: (cx - halfChord, py)
        if (cx - halfChord > px)
        {
            double angle = Math.Atan2(dy, -halfChord);
            if (IsAngleInArc(angle, startAngle, endAngle, ccw))
                winding += ccw ? -1 : 1;
        }

        return winding;
    }

    private static bool IsAngleInArc(double angle, double startAngle, double endAngle, bool ccw)
    {
        if (ccw)
        {
            double span = NormalizeAngle(endAngle - startAngle);
            double dist = NormalizeAngle(angle - startAngle);
            return dist <= span;
        }
        else
        {
            double span = NormalizeAngle(startAngle - endAngle);
            double dist = NormalizeAngle(startAngle - angle);
            return dist <= span;
        }
    }

    private static double NormalizeAngle(double angle)
    {
        angle %= (2 * Math.PI);
        if (angle < 0) angle += 2 * Math.PI;
        return angle;
    }

    private static int WindingBezier(Unit2D p0, Unit2D p1, Unit2D p2, Unit2D p3, Unit2D point, int depth)
    {
        double py = point.Y.Millimeters;
        double minY = Math.Min(Math.Min(p0.Y.Millimeters, p1.Y.Millimeters),
                               Math.Min(p2.Y.Millimeters, p3.Y.Millimeters));
        double maxY = Math.Max(Math.Max(p0.Y.Millimeters, p1.Y.Millimeters),
                               Math.Max(p2.Y.Millimeters, p3.Y.Millimeters));

        if (py < minY || py > maxY)
            return 0;

        const double flatnessTolerance = 0.05;
        const int maxDepth = 16;

        if (depth >= maxDepth || BezierFlatness(p0, p1, p2, p3) < flatnessTolerance)
            return WindingSegment(p0, p3, point);

        var m01   = Midpoint(p0, p1);
        var m12   = Midpoint(p1, p2);
        var m23   = Midpoint(p2, p3);
        var m012  = Midpoint(m01, m12);
        var m123  = Midpoint(m12, m23);
        var m0123 = Midpoint(m012, m123);

        return WindingBezier(p0, m01, m012, m0123, point, depth + 1)
             + WindingBezier(m0123, m123, m23, p3, point, depth + 1);
    }

    private static double BezierFlatness(Unit2D p0, Unit2D p1, Unit2D p2, Unit2D p3)
    {
        double dx = p3.X.Millimeters - p0.X.Millimeters;
        double dy = p3.Y.Millimeters - p0.Y.Millimeters;
        double len = Math.Sqrt(dx * dx + dy * dy);

        if (len < 1e-10)
        {
            double d1x = p1.X.Millimeters - p0.X.Millimeters, d1y = p1.Y.Millimeters - p0.Y.Millimeters;
            double d2x = p2.X.Millimeters - p0.X.Millimeters, d2y = p2.Y.Millimeters - p0.Y.Millimeters;
            return Math.Max(Math.Sqrt(d1x * d1x + d1y * d1y), Math.Sqrt(d2x * d2x + d2y * d2y));
        }

        double invLen = 1.0 / len;
        double dist1 = Math.Abs((p1.X.Millimeters - p0.X.Millimeters) * dy
                               - (p1.Y.Millimeters - p0.Y.Millimeters) * dx) * invLen;
        double dist2 = Math.Abs((p2.X.Millimeters - p0.X.Millimeters) * dy
                               - (p2.Y.Millimeters - p0.Y.Millimeters) * dx) * invLen;

        return Math.Max(dist1, dist2);
    }

    private static Unit2D Midpoint(Unit2D a, Unit2D b)
    {
        return new Unit2D(
            Unit.FromMillimeters((a.X.Millimeters + b.X.Millimeters) * 0.5),
            Unit.FromMillimeters((a.Y.Millimeters + b.Y.Millimeters) * 0.5));
    }

    private sealed class WindingWalker : IPolygonWalker
    {
        private readonly Unit2D _point;
        public int Winding { get; private set; }

        public WindingWalker(Unit2D point)
        {
            _point = point;
        }

        public void Begin(Unit2D startPoint) { }

        public void Line(Unit2D from, Unit2D to)
        {
            Winding += WindingSegment(from, to, _point);
        }

        public void Arc(Unit2D start, Unit2D mid, Unit2D end)
        {
            Winding += WindingArc(start, mid, end, _point);
        }

        public void Bezier(Unit2D from, Unit2D c1, Unit2D c2, Unit2D to)
        {
            Winding += WindingBezier(from, c1, c2, to, _point, 0);
        }

        public void AddLine(Unit2D from, Unit2D to)
        {
            Winding += WindingSegment(from, to, _point);
        }
    }
}
