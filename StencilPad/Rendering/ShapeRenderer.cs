using System.ComponentModel;
using System.Diagnostics;
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
    private Dictionary<IPolygon, ShapePolygonRenderer> _rendererMap;
    private GeometryGroup? _geometryGroup;
    private bool _geometryGroupDirty;
    private Pen? _pen;
    private Brush? _fill;
    private Transform? _transform;
    
    public ShapeRenderer(Shape shape, IResourceService resourceService)
    {
        _shape = shape;
        _shape.PolygonSet.PolygonAdded += AddPolygon;
        _shape.PolygonSet.PolygonRemoved += RemovePolygon;
        _shape.TransformChanged += OnTransformChanged;
        _shape.PropertyChanged += PropertyChanged;

        _resourceService = resourceService;
        _walker = new();
        
        _rendererMap = new();

        foreach (var polygon in _shape.PolygonSet)
        {
            AddPolygon(polygon);
        }

        UpdateProperties();

        _transform = _shape.Transform.CreateGroupTransform();
    }

    public override void Dispose()
    {
        foreach (var (polygon, _) in _rendererMap.ToList())
        {
            RemovePolygon(polygon);
        }

        _shape.PolygonSet.PolygonAdded -= AddPolygon;
        _shape.PolygonSet.PolygonRemoved -= RemovePolygon;
        _shape.TransformChanged -= OnTransformChanged;
        _shape.PropertyChanged -= PropertyChanged;
    }

    private void AddPolygon(IPolygon polygon)
    {
        var renderer = new ShapePolygonRenderer(polygon);

        renderer.StartCap = GetStartCapGeometry();
        renderer.EndCap = GetEndCapGeometry();
        renderer.LineWidth = _shape.LineWidth;
        renderer.RendererDirty += PolygonDirty;
        
        _rendererMap.Add(polygon, renderer);

        PolygonDirty();
    }

    private void RemovePolygon(IPolygon polygon)
    {
        if (_rendererMap.TryGetValue(polygon, out var renderer))
        {
            renderer.RendererDirty -= PolygonDirty;
            renderer.Dispose();
            _rendererMap.Remove(polygon);

            PolygonDirty();
        }
        else
        {
            Debug.WriteLine($"Attempted to remove polygon that was not in the renderer map: {polygon}");
        }
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

        foreach (var renderer in _rendererMap.Values)
        {
            renderer.StartCap = GetStartCapGeometry();
            renderer.EndCap = GetEndCapGeometry();
            renderer.LineWidth = _shape.LineWidth;
        }
    }

    private GeometryResource? GetStartCapGeometry()
    {
        if (_shape.StartCap == GeometryResourceId.None)
        {
            return null;
        }

        return _resourceService.Get(_shape.StartCap);
    }

    private GeometryResource? GetEndCapGeometry()
    {
        if (_shape.EndCap == GeometryResourceId.None)
        {
            return null;
        }

        return _resourceService.Get(_shape.EndCap);
    }

    private void PolygonDirty()
    {
        _geometryGroupDirty = true;
        InvokeRendererDirty();
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

    private void UpdateGeometryGroup()
    {
        if (!_geometryGroupDirty && _geometryGroup is not null)
        {
            return;
        }

        _geometryGroupDirty = false;
        
        _geometryGroup = new GeometryGroup
        {
            FillRule = FillRule.EvenOdd
        };
        
        foreach (var (polygon, renderer) in _rendererMap)
        {
            _geometryGroup.Children.Add(renderer.GetGeometry());
        }

        _geometryGroup.Freeze();
    }
    
    public override void Render(DrawingContext dc)
    {
        if (_pen is null || _fill is null || _transform is null)
        {
            return;
        }

        UpdateGeometryGroup();
        
        dc.PushTransform(_transform);
        dc.DrawGeometry(_fill, _pen, _geometryGroup);

        foreach (var (polygon, renderer) in _rendererMap)
        {
            renderer.RenderStartCap(dc, _fill, _pen);
            renderer.RenderEndCap(dc, _fill, _pen);
        }
        
        dc.Pop();
    }
}
