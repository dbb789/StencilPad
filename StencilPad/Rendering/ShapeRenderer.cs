using System.ComponentModel;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class ShapeRenderer : SheetElementRenderer
{
    public override Shape Element => _shape;
    public override UnitBounds SelectionBounds
    {
        get
        {
            return GetGeometryBounds().ApplyTransform(_shape.Transform);
        }
    }

    private readonly Shape _shape;
    private Pen? _pen;
    private Brush? _fill;
    private Transform? _transform;
    private StreamGeometry? _geometry;
    private UnitBounds _geometryBounds;
    private bool _geometryDirty;
    
    public ShapeRenderer(Shape shape)
    {
        _shape = shape;
        _shape.PolygonSet.PolygonAdded += PolygonAdded;
        _shape.PolygonSet.PolygonRemoved += PolygonRemoved;
        _shape.TransformChanged += OnTransformChanged;
        _shape.PropertyChanged += PropertyChanged;

        foreach (var polygon in _shape.PolygonSet)
        {
            polygon.GeometryChanged += MarkGeometryDirty;
        }

        UpdateProperties();
        RebuildGeometry();
        _geometryDirty = false;
    }

    public override void Dispose()
    {
        foreach (var polygon in _shape.PolygonSet)
        {
            polygon.GeometryChanged -= MarkGeometryDirty;
        }
        
        _shape.PolygonSet.PolygonAdded -= PolygonAdded;
        _shape.PolygonSet.PolygonRemoved -= PolygonRemoved;
        _shape.TransformChanged -= OnTransformChanged;
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

        return geometry.FillContains(_shape.Transform.InverseApply(unit).Millimeters);
    }

    public override bool BoundsTest(UnitBounds bounds)
    {
        // This is tricky because a rotated rectangle is not an axis-aligned rectangle in local space.
        // For simplicity, we'll check if any corner of the input bounds is inside the shape, 
        // OR if any corner of the shape's bounds is inside the input bounds.
        // Or more accurately, we can transform the input bounds into local space (it becomes a polygon)
        // but WPF's FillContainsWithDetail only takes a Geometry.
        
        var geometry = GetGeometry();
        
        // Transform the selection bounds into the local space of the shape.
        // It becomes a rotated rectangle.
        var localNW = _shape.Transform.InverseApply(bounds.NW);
        var localNE = _shape.Transform.InverseApply(bounds.NE);
        var localSW = _shape.Transform.InverseApply(bounds.SW);
        var localSE = _shape.Transform.InverseApply(bounds.SE);

        var localSelectionGeometry = new StreamGeometry();
        using (var ctx = localSelectionGeometry.Open())
        {
            ctx.BeginFigure(localNW.Millimeters, true, true);
            ctx.LineTo(localNE.Millimeters, true, false);
            ctx.LineTo(localSE.Millimeters, true, false);
            ctx.LineTo(localSW.Millimeters, true, false);
        }
        localSelectionGeometry.Freeze();

        return geometry.FillContainsWithDetail(localSelectionGeometry) != IntersectionDetail.Empty;
    }

    private void MarkGeometryDirty()
    {
        _geometryDirty = true;
        
        InvokeRendererDirty();
    }

    private void UpdateProperties()
    {
        _pen = new Pen(new SolidColorBrush(_shape.LineColor), _shape.LineWidth.Millimeters);
        _pen.Freeze();
        
        _fill = new SolidColorBrush(_shape.FillColor);
        _fill.Freeze();
        
        _transform = _shape.Transform.CreateGroupTransform();
    }

    private void PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _pen = new Pen(new SolidColorBrush(_shape.LineColor), _shape.LineWidth.Millimeters);
        _pen.Freeze();
        
        _fill = new SolidColorBrush(_shape.FillColor);
        _fill.Freeze();
        
        InvokeRendererDirty();
    }

    private void OnTransformChanged()
    {
        _transform = _shape.Transform.CreateGroupTransform();
        InvokeRendererDirty();
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

    private UnitBounds GetGeometryBounds()
    {
        if (_geometryDirty)
        {
            _geometryDirty = false;
            RebuildGeometry();
        }

        return _geometryBounds;
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
        
        _geometryBounds = UnitBounds.FromMinMax(
            new Unit2D(Unit.FromMillimeters(_geometry.Bounds.Left),
                       Unit.FromMillimeters(_geometry.Bounds.Top)),
            new Unit2D(Unit.FromMillimeters(_geometry.Bounds.Right),
                       Unit.FromMillimeters(_geometry.Bounds.Bottom)));
    }

    // Exposed so that ResourceService can build Geometry without needing to
    // know how to interpret a Shape object.
    public static void AddToGeometry(Shape shape, StreamGeometryContext ctx)
    {
        foreach (var polygon in shape.PolygonSet)
        {
            RendererUtil.AddToGeometry(ctx, polygon);
        }
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
