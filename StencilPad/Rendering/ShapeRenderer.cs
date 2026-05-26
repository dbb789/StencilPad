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
    private Pen? _pen;
    private Brush? _fill;
    private Transform? _transform;
    private StreamGeometry? _geometry;
    private bool _geometryDirty;
    
    public ShapeRenderer(Shape shape, IResourceService resourceService)
    {
        _shape = shape;
        _shape.PolygonSet.PolygonAdded += PolygonAdded;
        _shape.PolygonSet.PolygonRemoved += PolygonRemoved;
        _shape.TransformChanged += OnTransformChanged;
        _shape.PropertyChanged += PropertyChanged;

        _resourceService = resourceService;
        _walker = new();

        foreach (var polygon in _shape.PolygonSet)
        {
            polygon.GeometryChanged += MarkGeometryDirty;
        }

        UpdateProperties();

        _transform = _shape.Transform.CreateGroupTransform();

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
        _pen = new Pen(new SolidColorBrush(_shape.LineColor),
                       _shape.LineWidth.Millimeters);
        _pen.StartLineCap = PenLineCap.Square;
        _pen.EndLineCap = PenLineCap.Square;
        _pen.LineJoin = PenLineJoin.Miter;
        _pen.Freeze();
        
        _fill = new SolidColorBrush(_shape.FillColor);
        _fill.Freeze();        
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

        var startCap =_resourceService.Get(_shape.StartCap);
        var endCap = _resourceService.Get(_shape.EndCap);

        foreach (var polygon in _shape.PolygonSet)
        {
            if (!polygon.Closed && polygon.Vertices.Count > 1)
            {
                var startPosition = polygon.Vertices[0].Position;
                Unit2D startDirection;
                
                if (polygon.Edges[0].Type == EdgeType.Bezier)
                {
                    var bezier = BezierUtil.FromPolygonEdge(polygon, 0);

                    if (bezier.WalkRadius(0,
                                          1,
                                          0.1,
                                          0.0001,
                                          Unit.FromMillimeters(2.5),
                                          Unit.FromMillimeters(0.000001),
                                          out var startT))
                    {
                        startDirection = startPosition - bezier.At(startT);
                    }
                    else
                    {
                        startDirection = startPosition - polygon.Vertices[1].Position;
                    }
                }
                else
                {
                    startDirection = startPosition - polygon.Vertices[1].Position;
                }
                
                var startRotation = Math.Atan2(startDirection.Y.Millimeters,
                                               startDirection.X.Millimeters) * 180 / Math.PI;
                
                dc.PushTransform(new TranslateTransform(startPosition.X.Millimeters,
                                                        startPosition.Y.Millimeters));
                dc.PushTransform(new RotateTransform(startRotation + 90, 0, 0));
                dc.DrawGeometry(_fill, _pen, startCap);
                dc.Pop();
                dc.Pop();

                var endPosition = polygon.Vertices[^1].Position;
                Unit2D endDirection;

                if (polygon.Edges[^1].Type == EdgeType.Bezier)
                {
                    var bezier = BezierUtil.FromPolygonEdge(polygon, ^1);
                    
                    if (bezier.WalkRadius(1,
                                          0,
                                          -0.1,
                                          0.0001,
                                          Unit.FromMillimeters(2.5),
                                          Unit.FromMillimeters(0.0000001),
                                          out var endT))
                    {
                        endDirection = endPosition - bezier.At(endT);
                    }
                    else
                    {
                        endDirection = endPosition - polygon.Vertices[^2].Position;
                    }
                }
                else
                {
                    endDirection = endPosition - polygon.Vertices[^2].Position;
                }
                
                var endRotation = Math.Atan2(endDirection.Y.Millimeters, endDirection.X.Millimeters) * 180 / Math.PI;

                dc.PushTransform(new TranslateTransform(endPosition.X.Millimeters,
                                                        endPosition.Y.Millimeters));
                dc.PushTransform(new RotateTransform(endRotation + 90, 0, 0));
                dc.DrawGeometry(_fill, _pen, endCap);
                dc.Pop();
                dc.Pop();
            }
        }
        
        dc.Pop();
    }
}
