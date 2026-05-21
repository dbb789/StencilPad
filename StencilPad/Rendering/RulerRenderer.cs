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

    public override UnitBounds SelectionBounds
    {
        get
        {
            var localRulerBounds = UnitBounds.FromMinMax(
                new Unit2D(Unit.Min(_ruler.Min.X, _ruler.Max.X), Unit.Min(_ruler.Min.Y, _ruler.Max.Y)),
                new Unit2D(Unit.Max(_ruler.Min.X, _ruler.Max.X), Unit.Max(_ruler.Min.Y, _ruler.Max.Y)));

            var localBoundsWithTolerance = UnitBounds.FromCenterSize(
                localRulerBounds.Center,
                new Unit2D(Unit.Max(localRulerBounds.Size.X, Unit.FromMillimeters(5)),
                           Unit.Max(localRulerBounds.Size.Y, Unit.FromMillimeters(5))));

            return localBoundsWithTolerance.ApplyTransform(_ruler.Transform);
        }
    }

    private readonly Ruler _ruler;
    private readonly IResourceService _resourceService;
    private Transform? _transform;

    public RulerRenderer(Ruler ruler, IResourceService resourceService)
    {
        _ruler = ruler;
        _ruler.GeometryChanged += GeometryChanged;
        _ruler.TransformChanged += OnTransformChanged;
        _ruler.PropertyChanged += PropertyChanged;
        
        _resourceService = resourceService;
        UpdateProperties();
    }

    public override void Dispose()
    {
        _ruler.GeometryChanged -= GeometryChanged;
        _ruler.TransformChanged -= OnTransformChanged;
        _ruler.PropertyChanged -= PropertyChanged;
    }

    public override bool HitTest(Unit2D unit)
    {
        var localUnit = _ruler.Transform.InverseApply(unit);
        var start = _ruler.Min;
        var end = _ruler.Max;
        var lineX = end.X.Millimeters - start.X.Millimeters;
        var lineY = end.Y.Millimeters - start.Y.Millimeters;
        var lineLenSq = lineX * lineX + lineY * lineY;

        if (lineLenSq < 1e-10)
        {
            return false;
        }
        
        var toPointX = localUnit.X.Millimeters - start.X.Millimeters;
        var toPointY = localUnit.Y.Millimeters - start.Y.Millimeters;
        var t = Math.Clamp((toPointX * lineX + toPointY * lineY) / lineLenSq, 0.0, 1.0);

        var closestX = start.X.Millimeters + t * lineX;
        var closestY = start.Y.Millimeters + t * lineY;
        var dx = localUnit.X.Millimeters - closestX;
        var dy = localUnit.Y.Millimeters - closestY;

        return (dx * dx + dy * dy) < 4.0; // 2mm tolerance
    }

    public override bool BoundsTest(UnitBounds bounds)
    {
        // Transform the selection bounds into the local space of the ruler.
        var localNW = _ruler.Transform.InverseApply(bounds.NW);
        var localNE = _ruler.Transform.InverseApply(bounds.NE);
        var localSW = _ruler.Transform.InverseApply(bounds.SW);
        var localSE = _ruler.Transform.InverseApply(bounds.SE);

        var localSelectionBounds = UnitBounds.FromMinMax(
            new Unit2D(Unit.Min(Unit.Min(localNW.X, localNE.X), Unit.Min(localSW.X, localSE.X)),
                       Unit.Min(Unit.Min(localNW.Y, localNE.Y), Unit.Min(localSW.Y, localSE.Y))),
            new Unit2D(Unit.Max(Unit.Max(localNW.X, localNE.X), Unit.Max(localSW.X, localSE.X)),
                       Unit.Max(Unit.Max(localNW.Y, localNE.Y), Unit.Max(localSW.Y, localSE.Y))));

        return localSelectionBounds.Contains(_ruler.Min) || localSelectionBounds.Contains(_ruler.Max);
    }

    public override void Render(DrawingContext dc)
    {
        if (_transform is null)
        {
            return;
        }

        var start = _ruler.Min.Millimeters;
        var end = _ruler.Max.Millimeters;

        dc.PushTransform(_transform);
        dc.DrawLine(RulerPen, start, end);

        DrawArrowhead(dc, end, start);
        DrawArrowhead(dc, start, end);

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
        
        var rotation = Math.Atan2(end.Y - start.Y, end.X - start.X) * 180.0 / Math.PI;

        dc.PushTransform(new TranslateTransform(mid.X, mid.Y));
        dc.PushTransform(new RotateTransform(rotation));
        dc.DrawText(formattedText, new Point(-formattedText.Width / 2, 0.5));
        dc.Pop();
        dc.Pop();
        dc.Pop();
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

    private void PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        InvokeRendererDirty();
    }

    private void OnTransformChanged()
    {
        _transform = _ruler.Transform.CreateGroupTransform();
        InvokeRendererDirty();
    }

    private void UpdateProperties()
    {
        _transform = _ruler.Transform.CreateGroupTransform();
    }

    private void GeometryChanged()
    {
        InvokeRendererDirty();
    }
}
