using System.ComponentModel;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Rendering;

public class ShapeRenderer : SheetElementRenderer
{
    public override Shape Element => _shape;
    public override UnitBounds SelectionBounds
    {
        get
        {
            var geometry = GetGeometry();

            return UnitBounds.FromMinMax(
                new Unit2D(Unit.FromMillimeters(geometry.Bounds.Left),
                           Unit.FromMillimeters(geometry.Bounds.Top)),
                new Unit2D(Unit.FromMillimeters(geometry.Bounds.Right),
                           Unit.FromMillimeters(geometry.Bounds.Bottom))) + _shape.Position;
        }
    }

    private readonly Shape _shape;
    private Pen? _pen;
    private Brush? _fill;
    private Transform? _transform;
    private StreamGeometry? _geometry;
    private bool _geometryDirty;
    
    public ShapeRenderer(Shape shape)
    {
        _shape = shape;
        _shape.PolygonSet.PolygonAdded += PolygonAdded;
        _shape.PolygonSet.PolygonRemoved += PolygonRemoved;
        _shape.PropertyChanged += PropertyChanged;

        foreach (var polygon in _shape.PolygonSet)
        {
            polygon.GeometryChanged += MarkGeometryDirty;
        }

        UpdateProperties();

        _geometryDirty = true;
        GetGeometry();
    }

    public override void Dispose()
    {
        foreach (var polygon in _shape.PolygonSet)
        {
            polygon.GeometryChanged -= MarkGeometryDirty;
        }
        
        _shape.PolygonSet.PolygonAdded -= PolygonAdded;
        _shape.PolygonSet.PolygonRemoved -= PolygonRemoved;
        _shape.PropertyChanged -= PropertyChanged;
    }

    private void PolygonAdded(EditablePolygon polygon)
    {
        polygon.GeometryChanged += MarkGeometryDirty;
        MarkGeometryDirty();
    }

    private void PolygonRemoved(EditablePolygon polygon)
    {
        polygon.GeometryChanged -= MarkGeometryDirty;
        MarkGeometryDirty();
    }

    public override bool HitTest(Unit2D unit)
    {
        var geometry = GetGeometry();

        return geometry.FillContains((unit -_shape.Position).Millimeters);
    }

    public override bool BoundsTest(UnitBounds bounds)
    {
        var geometry = GetGeometry();
        var rect = new RectangleGeometry((bounds -_shape.Position).Millimeters);

        return geometry.FillContainsWithDetail(rect) != IntersectionDetail.Empty;
    }

    private void PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateProperties();
        InvokeInvalidateVisual();
    }

    private void MarkGeometryDirty()
    {
        _geometryDirty = true;
        
        InvokeInvalidateVisual();
    }

    private void UpdateProperties()
    {
        _pen = new Pen(new SolidColorBrush(_shape.LineColor), _shape.LineWidth.Millimeters);
        _fill = new SolidColorBrush(_shape.FillColor);
        _transform = new TranslateTransform(_shape.Position.X.Millimeters,
                                            _shape.Position.Y.Millimeters);
    }

    private Geometry GetGeometry()
    {
        if (_geometryDirty)
        {
            _geometryDirty = false;
            RebuildGeometry();
        }

        return _geometry!;
    }
    
    private void RebuildGeometry()
    {
        _geometry = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        using (var ctx = _geometry.Open())
        {
            foreach (var polygon in _shape.PolygonSet)
            {
                RendererUtil.AddToGeometry(ctx, polygon);
            }
        }

        _geometry.Freeze();
    }

    public override void Render(DrawingContext dc)
    {

        if (_pen is null || _fill is null || _transform is null)
        {
            return;
        }

        var geometry = GetGeometry();
        
        dc.PushTransform(_transform);
        dc.DrawGeometry(_fill, _pen, geometry);
        dc.Pop();
    }
}
