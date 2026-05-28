using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class RulerRenderer : SheetElementRenderer
{
    public override Ruler Element => _ruler;

    private readonly Ruler _ruler;
    private readonly IResourceService _resourceService;
    private Transform? _transform;
    private Pen _pen = new(Brushes.Black, 0.2);
    private Brush _brush = Brushes.Black;

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

        var start = _ruler.Min;
        var end = _ruler.Max;

        var geometry = _resourceService.Get(GeometryResourceId.First);
        var offset = end - start;
        
        dc.PushTransform(_transform);
        
        DrawCap(dc, geometry.Geometry, end, start);
        DrawCap(dc, geometry.Geometry, start, end);

        start += offset.NormalizedTo(geometry.Size.Y);
        end -= offset.NormalizedTo(geometry.Size.Y);

        dc.DrawLine(_pen, start.Millimeters, end.Millimeters);
        
        var mid = new Unit2D((start.X + end.X) / 2.0, (start.Y + end.Y) / 2.0);
        var label = $"{_ruler.Length.Millimeters:F1} mm";

        var formattedText = new FormattedText(
            label,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            3.0,
            _brush,
            1.0);

        var rotation = Math.Atan2((end.Y - start.Y).Millimeters,
                                  (end.X - start.X).Millimeters) * 180.0 / Math.PI;

        dc.PushTransform(new TranslateTransform(mid.X.Millimeters, mid.Y.Millimeters));
        dc.PushTransform(new RotateTransform(rotation));
        dc.DrawText(formattedText, new Point(-formattedText.Width / 2, 0.5));
        dc.Pop();
        dc.Pop();

        dc.Pop();
    }

    private void DrawCap(DrawingContext dc, Geometry geometry, Unit2D tipUnits, Unit2D fromUnits)
    {
        var tip = tipUnits.Millimeters;
        var from = fromUnits.Millimeters;
        var rotation = Math.Atan2(from.Y - tip.Y, from.X - tip.X) * 180.0 / Math.PI;

        rotation -= 90.0;

        dc.PushTransform(new TranslateTransform(tip.X, tip.Y));
        dc.PushTransform(new RotateTransform(rotation));
        dc.DrawGeometry(_brush, null, geometry);
        dc.Pop();
        dc.Pop();
    }

    private void PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateProperties();
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
        _brush = new SolidColorBrush(_ruler.Color);
        _brush.Freeze();
        _pen = new Pen(_brush, 0.2);
        _pen.Freeze();
    }

    private void GeometryChanged(ISheetElement _)
    {
        InvokeRendererDirty();
    }
}
