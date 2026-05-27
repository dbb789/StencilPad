using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class ShapePolygonRenderer
{
    public GeometryResource? StartCap
    {
        get => _startCap;
        set
        {
            if (_startCap != value)
            {
                _startCap = value;
                MarkGeometryDirty(_polygon);
            }
        }
    }

    public GeometryResource? EndCap
    {
        get => _endCap;
        set
        {
            if (_endCap != value)
            {
                _endCap = value;
                MarkGeometryDirty(_polygon);
            }
        }
    }

    private readonly IPolygon _polygon;
    private readonly StreamGeometryWalker _geometryWalker;
    
    private CapDistanceWalker? _capWalker;
    private ClampedGeometryWalker? _clampedWalker;
    private Geometry _geometry;
    private GeometryResource? _startCap;
    private GeometryResource? _endCap;
    private Transform _startCapTransform = Transform.Identity;
    private Transform _endCapTransform = Transform.Identity;
    private bool _geometryDirty;

    public event Action? RendererDirty;
    
    public ShapePolygonRenderer(IPolygon polygon)
    {
        _polygon = polygon;
        _geometryWalker = new StreamGeometryWalker();
        _geometry = BuildGeometry(_polygon);

        _polygon.GeometryChanged += MarkGeometryDirty;
    }

    public void Dispose()
    {
        _polygon.GeometryChanged -= MarkGeometryDirty;
    }

    public Geometry GetGeometry()
    {
        if (_geometryDirty)
        {
            _geometry = BuildGeometry(_polygon);
            _geometryDirty = false;
        }

        return _geometry;
    }

    public void RenderStartCap(DrawingContext dc, Pen pen)
    {
        if (_polygon.Vertices.Count == 0 || _polygon.Closed)
        {
            return;
        }

        if (_startCap is not null)
        {
            dc.PushTransform(_startCapTransform);
            dc.DrawGeometry(null, pen, _startCap.Geometry);
            dc.Pop();
        }
    }
    
    public void RenderEndCap(DrawingContext dc, Pen pen)
    {
        if (_polygon.Vertices.Count == 0 || _polygon.Closed)
        {
            return;
        }
        
        if (_endCap is not null)
        {
            dc.PushTransform(_endCapTransform);
            dc.DrawGeometry(null, pen, _endCap.Geometry);
            dc.Pop();
        }
    }

    private void MarkGeometryDirty(IPolygon polygon)
    {
        _geometryDirty = true;
        RendererDirty?.Invoke();
    }

    private Geometry BuildGeometry(IPolygon polygon)
    {
        var geometry = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        using (var ctx = geometry.Open())
        {
            _geometryWalker.Context = ctx;

            if (!polygon.Closed)
            {
                SegmentPoint? startPoint = null;
                SegmentPoint? endPoint = null;
                
                if (_startCap is not null)
                {
                    _capWalker ??= new CapDistanceWalker();
                    _capWalker.Reset(_startCap.Bounds.Size.Y);

                    polygon.Resolver.WalkPolygon(_capWalker);

                    startPoint = _capWalker.Point;
                }

                if (_endCap is not null)
                {
                    _capWalker ??= new CapDistanceWalker();
                    _capWalker.Reset(_endCap.Bounds.Size.Y);

                    polygon.Resolver.WalkPolygonReverse(_capWalker);

                    endPoint = _capWalker.Point;

                    if (endPoint is not null)
                    {
                        endPoint = endPoint.Value with { Fraction = 1.0 - endPoint.Value.Fraction };
                    }
                }

                if (startPoint is not null || endPoint is not null)
                {
                    _clampedWalker ??= new ClampedGeometryWalker(_geometryWalker);
                    _clampedWalker.SetStartEnd(startPoint, endPoint);

                    polygon.Resolver.WalkPolygon(_clampedWalker);
                }
                else
                {
                    polygon.Resolver.WalkPolygon(_geometryWalker);
                }
            }
            else
            {
                polygon.Resolver.WalkPolygon(_geometryWalker);
            }
        }

        _startCapTransform = BuildStartCapTransform();
        _endCapTransform = BuildEndCapTransform();
        
        geometry.Freeze();

        return geometry;
    }
    
    private Transform BuildStartCapTransform()
    {
        if (_polygon.Vertices.Count == 0 ||
            _polygon.Closed)
        {
            return Transform.Identity;
        }
        
        var position = _polygon.Vertices[0].Position;
        var offset = position - _geometryWalker.StartPosition;
        var rotation = Math.Atan2(offset.Y.Millimeters,
                                  offset.X.Millimeters) * 180 / Math.PI;

        var rotateTransform = new RotateTransform(rotation + 90, 0, 0);

        rotateTransform.Freeze();

        var translateTransform = new TranslateTransform(position.X.Millimeters,
                                                        position.Y.Millimeters);

        translateTransform.Freeze();
        
        var transformGroup = new TransformGroup();
        
        transformGroup.Children.Add(rotateTransform);
        transformGroup.Children.Add(translateTransform);
        transformGroup.Freeze();
        
        return transformGroup;
    }

    private Transform BuildEndCapTransform()
    {
        if (_polygon.Vertices.Count == 0 ||
            _polygon.Closed)
        {
            return Transform.Identity;
        }

        var position = _polygon.Vertices[^1].Position;
        var offset = position - _geometryWalker.EndPosition;
        var rotation = Math.Atan2(offset.Y.Millimeters,
                                  offset.X.Millimeters) * 180 / Math.PI;

        var rotateTransform = new RotateTransform(rotation + 90, 0, 0);

        rotateTransform.Freeze();
        
        var translateTransform = new TranslateTransform(position.X.Millimeters,
                                                        position.Y.Millimeters);

        translateTransform.Freeze();
        
        var transformGroup = new TransformGroup();

        transformGroup.Children.Add(rotateTransform);
        transformGroup.Children.Add(translateTransform);
        transformGroup.Freeze();
        
        return transformGroup;
    }
}
