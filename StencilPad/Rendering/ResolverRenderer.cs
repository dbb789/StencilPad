using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Models.Resolvers;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class ResolverRenderer : SheetElementRenderer, IStyledGeometryWalker
{
    private class Entry
    {
        public GeometrySet GeometrySet;

        ////////////////////
        
        public Geometry? Geometry;
        public bool GeometryDirty;
    }

    private readonly IStyledGeometryResolver _resolver;
    private readonly IResourceService _resourceService;
    private readonly Dictionary<int, Entry> _entryMap;

    private ClampedGeometryWalker? _clampedGeometryWalker;
    private StreamGeometryWalker? _streamGeometryWalker;
    private Transform? _transform;
    private Pen? _pen;
    private Brush? _brush;

    public ResolverRenderer(IStyledGeometryResolver resolver,
                            IResourceService resourceService)
    {
        _resolver = resolver;
        _resourceService = resourceService;
        _entryMap = new();

        _resolver.Subscribe(this);
    }

    public override void Dispose()
    {
        _resolver.Unsubscribe(this);
    }

    public override void Render(DrawingContext dc)
    {
        if (_pen is null ||
            _brush is null ||
            _transform is null)
        {
            return;
        }
        
        var geometryGroup = new GeometryGroup
        {
            FillRule = FillRule.EvenOdd
        };

        foreach (var (_, entry) in _entryMap)
        {
            geometryGroup.Children.Add(GetGeometry(entry));
        }

        geometryGroup.Freeze();
        
        dc.PushTransform(_transform);
        dc.DrawGeometry(_brush, _pen, geometryGroup);
        
        foreach (var (_, entry) in _entryMap)
        {
            foreach (var (resource, overlayTransform) in entry.GeometrySet.Overlays)
            {
                dc.PushTransform(overlayTransform.CreateGroupTransform());
                dc.DrawGeometry(_brush, _pen, resource.Geometry);
                dc.Pop();
            }
        }
        
        dc.Pop();
    }
    
    public void SetStyle(GeometryStyle style)
    {
        _pen = CreatePen(style);
        _brush = CreateBrush(style);

        InvokeRendererDirty();
    }
    
    public void SetTransform(UnitTransform transform)
    {
        _transform = transform.CreateGroupTransform();

        InvokeRendererDirty();
    }
    
    public void Create(int id,
                       GeometrySet geometry)
    {
        _entryMap[id] = new Entry
        {
            GeometrySet = geometry,
            Geometry = null,
            GeometryDirty = true,
        };
    }

    public void Update(int id,
                       GeometrySet geometry)
    {
        if (_entryMap.TryGetValue(id, out var entry))
        {
            entry.GeometrySet = geometry;
            entry.GeometryDirty = true;
        }

        InvokeRendererDirty();
    }

    public void Destroy(int id)
    {
        _entryMap.Remove(id);
    }

    private Geometry GetGeometry(Entry entry)
    {
        if (!entry.GeometryDirty && entry.Geometry is not null)
        {
            return entry.Geometry;
        }

        entry.GeometryDirty = false;
        
        var geometry = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        using (var ctx = geometry.Open())
        {
            _streamGeometryWalker ??= new StreamGeometryWalker();
            _streamGeometryWalker.Context = ctx;
            
            if (entry.GeometrySet.StartPoint is not null ||
                entry.GeometrySet.EndPoint is not null)
            {
                _clampedGeometryWalker ??= new ClampedGeometryWalker(_streamGeometryWalker);
                _clampedGeometryWalker.SetStartEnd(entry.GeometrySet.StartPoint,
                                                   entry.GeometrySet.EndPoint);

                entry.GeometrySet.Resolver.Walk(_clampedGeometryWalker);
            }
            else
            {
                entry.GeometrySet.Resolver.Walk(_streamGeometryWalker);
            }
        }

        geometry.Freeze();

        entry.Geometry = geometry;

        return geometry;
    }

    private Pen CreatePen(GeometryStyle style)
    {
        var pen = new Pen(new SolidColorBrush(style.LineColor),
                          style.LineWidth.Millimeters);

        pen.StartLineCap = PenLineCap.Flat;
        pen.EndLineCap = PenLineCap.Flat;
        pen.LineJoin = PenLineJoin.Miter;
        pen.DashStyle = _resourceService.Get(style.LineStyle);
        
        pen.Freeze();
        
        return pen;
    }

    private Brush CreateBrush(GeometryStyle style)
    {
        var brush = new SolidColorBrush(style.FillColor);
        
        brush.Freeze();
        
        return brush;
    }
}
