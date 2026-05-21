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

    private void OnTransformChanged(ISheetElement element)
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
