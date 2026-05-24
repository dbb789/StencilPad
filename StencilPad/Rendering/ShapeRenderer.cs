using System.ComponentModel;
using System.Windows.Media;
using StencilPad.Models;

namespace StencilPad.Rendering;

public class ShapeRenderer : SheetElementRenderer
{
    public override Shape Element => _shape;

    private readonly Shape _shape;
    private readonly StreamGeometryWalker _walker;
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
        _shape.TransformChanged += OnTransformChanged;
        _shape.PropertyChanged += PropertyChanged;

        _walker = new();

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

    private void OnTransformChanged(ISheetElement element)
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

    private void RebuildGeometry()
    {
        _geometry = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        using (var ctx = _geometry.Open())
        {
            _walker.Context = ctx;
            
            foreach (var polygon in _shape.PolygonSet)
            {
                polygon.Resolver.WalkPolygon(_walker);
            }
        }

        _geometry.Freeze();
    }

    // Exposed so that ResourceService can build Geometry without needing to
    // know how to interpret a Shape object.
    public static void AddToGeometry(Shape shape, StreamGeometryContext ctx)
    {
        var walker = new StreamGeometryWalker
        {
            Context = ctx
        };
        
        foreach (var polygon in shape.PolygonSet)
        {
            polygon.Resolver.WalkPolygon(walker);
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
