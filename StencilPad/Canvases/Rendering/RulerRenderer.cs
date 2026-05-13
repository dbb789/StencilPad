using System.Globalization;
using System.Windows;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Rendering;

public class RulerRenderer : SheetElementRenderer
{
    public override Ruler Element => _ruler;

    public override UnitBounds SelectionBounds =>
        UnitBounds.FromMinMax(
            new Unit2D(Unit.Min(_ruler.Start.X, _ruler.End.X), Unit.Min(_ruler.Start.Y, _ruler.End.Y)),
            new Unit2D(Unit.Max(_ruler.Start.X, _ruler.End.X), Unit.Max(_ruler.Start.Y, _ruler.End.Y)));

    private readonly Ruler _ruler;

    public RulerRenderer(Ruler ruler)
    {
        _ruler = ruler;
        _ruler.GeometryChanged += GeometryChanged;
    }

    public override void Dispose()
    {
        _ruler.GeometryChanged -= GeometryChanged;
    }

    public override bool HitTest(Unit2D unit)
    {
        var start = _ruler.Start;
        var end = _ruler.End;
        var lineX = end.X.Millimeters - start.X.Millimeters;
        var lineY = end.Y.Millimeters - start.Y.Millimeters;
        var lineLenSq = lineX * lineX + lineY * lineY;

        if (lineLenSq < 1e-10)
        {
            return false;
        }
        
        var toPointX = unit.X.Millimeters - start.X.Millimeters;
        var toPointY = unit.Y.Millimeters - start.Y.Millimeters;
        var t = Math.Clamp((toPointX * lineX + toPointY * lineY) / lineLenSq, 0.0, 1.0);

        var closestX = start.X.Millimeters + t * lineX;
        var closestY = start.Y.Millimeters + t * lineY;
        var dx = unit.X.Millimeters - closestX;
        var dy = unit.Y.Millimeters - closestY;

        return (dx * dx + dy * dy) < 4.0; // 2mm tolerance
    }

    public override bool BoundsTest(UnitBounds bounds)
    {
        return bounds.Contains(_ruler.Start) || bounds.Contains(_ruler.End);
    }

    public override void Render(DrawingContext dc)
    {
        var start = _ruler.Start.Millimeters;
        var end = _ruler.End.Millimeters;

        var pen = new Pen(Brushes.Black, 0.2);

        dc.DrawLine(pen, start, end);

        DrawArrowhead(dc, tip: end, from: start);
        DrawArrowhead(dc, tip: start, from: end);

        var mid = new Point((start.X + end.X) / 2.0, (start.Y + end.Y) / 2.0);
        var label = $"{_ruler.Length.Millimeters:F1} mm";

        var formattedText = new FormattedText(
            label,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            3.0,
            Brushes.Black,
            1.0);

        dc.DrawText(formattedText, new Point(mid.X - formattedText.Width / 2.0, mid.Y + 1.5));
    }

    private static void DrawArrowhead(DrawingContext dc, Point tip, Point from)
    {
        const double arrowLength = 2.5;
        const double arrowHalfWidth = 1.0;

        var dx = tip.X - from.X;
        var dy = tip.Y - from.Y;
        var len = Math.Sqrt(dx * dx + dy * dy);

        if (len < 1e-10)
        {
            return;
        }
        
        dx /= len;
        dy /= len;

        var baseX = tip.X - dx * arrowLength;
        var baseY = tip.Y - dy * arrowLength;

        var p1 = new Point(baseX + -dy * arrowHalfWidth, baseY + dx * arrowHalfWidth);
        var p2 = new Point(baseX - -dy * arrowHalfWidth, baseY - dx * arrowHalfWidth);

        var geometry = new StreamGeometry();
        
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(tip, isFilled: true, isClosed: true);
            ctx.LineTo(p1, isStroked: false, isSmoothJoin: false);
            ctx.LineTo(p2, isStroked: false, isSmoothJoin: false);
        }
        
        geometry.Freeze();

        dc.DrawGeometry(Brushes.Black, null, geometry);
    }

    private void GeometryChanged()
    {
        InvokeInvalidateVisual();
    }
}
