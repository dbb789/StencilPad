using System.Windows.Media;
using StencilPad.Models.Resolvers;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class StyledGeometryRenderer : IStyledGeometryWalker, IWalkerRenderer
{
    private class Entry
    {
        public GeometrySet GeometrySet;

        ////////////////////
        
        public Geometry? Geometry;
        public bool GeometryDirty;
    }

    private readonly IResourceService _resourceService;
    private readonly Dictionary<int, Entry> _entryMap;
    private ClampedGeometryWalker? _clampedGeometryWalker;
    private StreamGeometryWalker? _streamGeometryWalker;
    
    private GeometryGroup? _baseGeometry;
    private bool _geometryDirty;
    private Pen? _pen;
    private Brush? _brush;

    public event Action? RendererDirty;
    
    public StyledGeometryRenderer(IResourceService resourceService)
    {
        _resourceService = resourceService;
        _entryMap = new();
        _geometryDirty = true;
    }

    public void Dispose()
    {
    }

    public void Render(DrawingContext dc)
    {
        if (_pen is null || _brush is null)
        {
            return;
        }

        var geometry = GetGeometryGroup();

        dc.DrawGeometry(_brush, _pen, geometry);
        
        foreach (var (_, entry) in _entryMap)
        {
            foreach (var (resource, overlayTransform) in entry.GeometrySet.Overlays)
            {
                dc.PushTransform(overlayTransform.CreateGroupTransform());
                dc.DrawGeometry(_brush, _pen, resource.Geometry);
                dc.Pop();
            }
        }
    }
    
    public void SetStyle(GeometryStyle style)
    {
        _pen = CreatePen(style);
        _brush = CreateBrush(style);

        InvokeRendererDirty();
    }
    
    public void Create(int id, GeometrySet geometry)
    {
        _entryMap[id] = new Entry
        {
            GeometrySet = geometry,
            Geometry = null,
            GeometryDirty = true,
        };

        _geometryDirty = true;
        
        InvokeRendererDirty();
    }

    public void Update(int id, GeometrySet geometry)
    {
        if (!_entryMap.TryGetValue(id, out var entry))
        {
            return;
        }
        
        entry.GeometrySet = geometry;
        entry.GeometryDirty = true;

        _geometryDirty = true;

        InvokeRendererDirty();
    }

    public void Destroy(int id)
    {
        _entryMap.Remove(id);
        _geometryDirty = true;

        InvokeRendererDirty();
    }

    private Geometry GetGeometryGroup()
    {
        if (!_geometryDirty && _baseGeometry is not null)
        {
            return _baseGeometry;
        }
        
        _geometryDirty = false;
            
        _baseGeometry = new GeometryGroup
        {
            FillRule = FillRule.EvenOdd
        };

        foreach (var (_, entry) in _entryMap)
        {
            _baseGeometry.Children.Add(GetGeometry(entry));
        }

        _baseGeometry.Freeze();

        return _baseGeometry;
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

    private void InvokeRendererDirty()
    {
        RendererDirty?.Invoke();
    }
}
