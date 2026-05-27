using System.ComponentModel;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class ShapeRenderer : SheetElementRenderer
{
    public override Shape Element => _shape;

    private readonly Shape _shape;
    private readonly IResourceService _resourceService;
    private readonly StreamGeometryWalker _walker;
    private Dictionary<IPolygon, Geometry?> _geometryMap;
    private Pen? _pen;
    private Brush? _fill;
    private Brush? _capFill;
    private Transform? _transform;
    
    public ShapeRenderer(Shape shape, IResourceService resourceService)
    {
        _shape = shape;
        _shape.PolygonSet.PolygonAdded += PolygonAdded;
        _shape.PolygonSet.PolygonRemoved += PolygonRemoved;
        _shape.TransformChanged += OnTransformChanged;
        _shape.PropertyChanged += PropertyChanged;

        _resourceService = resourceService;
        _walker = new();
        
        _geometryMap = new();

        foreach (var polygon in _shape.PolygonSet)
        {
            polygon.GeometryChanged += MarkGeometryDirty;
            _geometryMap[polygon] = BuildGeometry(polygon);
        }

        UpdateProperties();

        _transform = _shape.Transform.CreateGroupTransform();
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
        MarkGeometryDirty(polygon);
    }

    private void PolygonRemoved(EditablePolygon polygon)
    {
        polygon.GeometryChanged -= MarkGeometryDirty;
        _geometryMap.Remove(polygon);
        InvokeRendererDirty();
    }

    private void MarkGeometryDirty(IPolygon polygon)
    {
        _geometryMap[polygon] = null;
        
        InvokeRendererDirty();
    }

    private void UpdateProperties()
    {
        _pen = new Pen(new SolidColorBrush(_shape.LineColor),
                       _shape.LineWidth.Millimeters);
        _pen.StartLineCap = PenLineCap.Flat;
        _pen.EndLineCap = PenLineCap.Flat;
        _pen.LineJoin = PenLineJoin.Miter;
        _pen.DashStyle = _resourceService.Get(_shape.LineStyle);
        
        _pen.Freeze();
        
        _fill = new SolidColorBrush(_shape.FillColor);
        _fill.Freeze();

        _capFill = new SolidColorBrush(_shape.LineColor);
        _capFill.Freeze();
    }

    private void PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateProperties();
        InvokeRendererDirty();
    }

    private void OnTransformChanged(ISheetElement element)
    {
        _transform = _shape.Transform.CreateGroupTransform();
        
        InvokeRendererDirty();
    }

    private Geometry BuildGeometry(IPolygon polygon)
    {
        var geometry = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        using (var ctx = geometry.Open())
        {
            _walker.Context = ctx;
            
            polygon.Resolver.WalkPolygon(_walker);
        }

        geometry.Freeze();

        return geometry;
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

        dc.PushTransform(_transform);

        var gg = new GeometryGroup
        {
            FillRule = FillRule.EvenOdd
        };
        
        foreach (var entry in _geometryMap)
        {
            var polygon = entry.Key;
            var geometry = entry.Value;

            if (geometry is null)
            {
                geometry = BuildGeometry(polygon);
                _geometryMap[polygon] = geometry;
            }

            gg.Children.Add(geometry);

            if (!polygon.Closed && polygon.Vertices.Count > 1)
            {
                var capScale = _shape.LineWidth.Millimeters / 0.2;
                
                if (_shape.StartCap != GeometryResourceId.None)
                {
                    var startCap = _resourceService.Get(_shape.StartCap);
                    var (startPosition, startRotation) = GetStartCapTransform(polygon);

                    RenderCap(dc, _shape.StartCap, startPosition, startRotation, capScale);
                }

                if (_shape.EndCap != GeometryResourceId.None)
                {
                    var endCap = _resourceService.Get(_shape.EndCap);
                    var (endPosition, endRotation) = GetEndCapTransform(polygon);
                    
                    RenderCap(dc, _shape.EndCap, endPosition, endRotation, capScale);
                }
            }
        }

        gg.Freeze();
        
        dc.DrawGeometry(_fill, _pen, gg);
        
        dc.Pop();
    }

    private void RenderCap(DrawingContext dc,
                           GeometryResourceId cap,
                           Unit2D position,
                           double rotationDegrees,
                           double scale)
    {
        if (cap == GeometryResourceId.None)
        {
            return;
        }

        var geometry = _resourceService.Get(cap);
        
        dc.PushTransform(new TranslateTransform(position.X.Millimeters,
                                                position.Y.Millimeters));
        dc.PushTransform(new RotateTransform(rotationDegrees + 90, 0, 0));
        dc.PushTransform(new ScaleTransform(scale, scale, 0, 0));
        dc.DrawGeometry(_capFill, null, geometry);
        dc.Pop();
        dc.Pop();
        dc.Pop();
    }

    private (Unit2D, double) GetStartCapTransform(IPolygon polygon)
    {
        var position = polygon.Vertices[0].Position;
        Unit2D direction;
        
        if (polygon.Edges[0].Type == EdgeType.Bezier)
        {
            var bezier = BezierUtil.FromPolygonEdge(polygon, 0);
            
            if (bezier.WalkRadius(0,
                                  1,
                                  0.1,
                                  0.0001,
                                  Unit.FromMillimeters(2.5),
                                  Unit.FromMillimeters(0.000001),
                                  out var t))
            {
                direction = position - bezier.At(t);
            }
            else
            {
                direction = position - polygon.Vertices[1].Position;
            }
        }
        else
        {
            direction = position - polygon.Vertices[1].Position;
        }
        
        var rotation = Math.Atan2(direction.Y.Millimeters,
                                  direction.X.Millimeters) * 180 / Math.PI;

        return (position, rotation);
    }

    private (Unit2D, double) GetEndCapTransform(IPolygon polygon)
    {
        var position = polygon.Vertices[^1].Position;
        Unit2D direction;

        if (polygon.Edges[^1].Type == EdgeType.Bezier)
        {
            var bezier = BezierUtil.FromPolygonEdge(polygon, ^1);

            if (bezier.WalkRadius(1,
                                  0,
                                  -0.1,
                                  0.0001,
                                  Unit.FromMillimeters(2.5),
                                  Unit.FromMillimeters(0.0000001),
                                  out var t))
            {
                direction = position - bezier.At(t);
            }
            else
            {
                direction = position - polygon.Vertices[^2].Position;
            }
        }
        else
        {
            direction = position - polygon.Vertices[^2].Position;
        }

        var rotation = Math.Atan2(direction.Y.Millimeters,
                                  direction.X.Millimeters) * 180 / Math.PI;

        return (position, rotation);
    }
}
