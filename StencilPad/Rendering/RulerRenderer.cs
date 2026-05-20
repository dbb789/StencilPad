using System.Globalization;
using System.Windows;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class RulerRenderer : SheetElementRenderer
{
    private static Pen RulerPen;

    static RulerRenderer()
    {
        RulerPen = new Pen(Brushes.Black, 0.2);
        RulerPen.Freeze();
    }

    public override Ruler Element => _ruler;

    public override UnitBounds SelectionBounds =>
        UnitBounds.FromMinMax(
            new Unit2D(Unit.Min(_ruler.Min.X, _ruler.Max.X), Unit.Min(_ruler.Min.Y, _ruler.Max.Y)),
            new Unit2D(Unit.Max(_ruler.Min.X, _ruler.Max.X), Unit.Max(_ruler.Min.Y, _ruler.Max.Y)));

    private readonly Ruler _ruler;
    private readonly IResourceService _resourceService;

    public RulerRenderer(Ruler ruler, IResourceService resourceService)
    {
        _ruler = ruler;
        _ruler.GeometryChanged += GeometryChanged;
        
        _resourceService = resourceService;
    }

    public override void Dispose()
    {
        _ruler.GeometryChanged -= GeometryChanged;
    }

    public override bool HitTest(Unit2D unit)
    {
        var start = _ruler.Min;
        var end = _ruler.Max;
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
        return bounds.Contains(_ruler.Min) || bounds.Contains(_ruler.Max);
    }

    public override void Render(DrawingContext dc)
    {
        var start = _ruler.Min.Millimeters;
        var end = _ruler.Max.Millimeters;

        dc.DrawLine(RulerPen, start, end);

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

    private void DrawArrowhead(DrawingContext dc, Point tip, Point from)
    {
        var geometry = _resourceService.Get(GeometryResourceId.Arrow0);
        var rotation = Math.Atan2(from.Y - tip.Y, from.X - tip.X) * 180.0 / Math.PI;

        rotation -= 90.0;
        
        dc.PushTransform(new TranslateTransform(tip.X, tip.Y));
        dc.PushTransform(new RotateTransform(rotation));
        dc.PushTransform(new ScaleTransform(0.25, 0.25));
        dc.DrawGeometry(Brushes.Black, null, geometry);
        dc.Pop();
        dc.Pop();
        dc.Pop();
    }

    private void GeometryChanged()
    {
        InvokeInvalidateVisual();
    }
}
