using System.Diagnostics;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Rendering;
using StencilPad.Schemas;
using StencilPad.Spatial;

namespace StencilPad.Services;

public class ResourceService : IResourceService
{
    private Dictionary<GeometryResourceId, GeometryResource> _geometryMap;
    private Dictionary<GeometryResourceType, List<GeometryResourceId>> _byType;
    private Dictionary<LineStyleResourceId, DashStyle> _lineStyleMap;

    public ResourceService()
    {
        _geometryMap = [];
        _byType = [];
        _lineStyleMap = [];
        
        LoadGeometry();
        LoadLineStyles();
    }

    public IEnumerable<GeometryResourceId> GetGeometryResourceIds(GeometryResourceType type)
    {
        if (_byType.TryGetValue(type, out var list))
        {
            return list;
        }

        return Enumerable.Empty<GeometryResourceId>();
    }

    public IEnumerable<LineStyleResourceId> GetLineStyleResourceIds()
    {
        return LineStyleResourceLibrary.ResourceList.Select(entry => entry.Item1);
    }

    private void LoadGeometry()
    {
        foreach (var entry in GeometryResourceLibrary.Load())
        {
            Load(entry.Id, entry.Filename, entry.Size);

            List<GeometryResourceId> list;
            
            if (!_byType.TryGetValue(entry.Type, out list!))
            {
                list = [];
                _byType[entry.Type] = list;
            }

            list.Add(entry.Id);
        }        
    }

    private void LoadLineStyles()
    {
        foreach (var entry in LineStyleResourceLibrary.ResourceList)
        {
            _lineStyleMap[entry.Item1] = entry.Item2;
        }
    }

    private void Load(GeometryResourceId id, string filename, Unit2D? size)
    {
        Geometry? geometry = null;
        Shape? shape = null;
        Unit2D geometrySize = Unit2D.Zero;
        
        try
        {
            (geometry, shape, geometrySize) = LoadGeometry(filename);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error loading geometry resource '{id}' from file '{filename}': {e.Message}");
            return;
        }
        
        if (geometry != null)
        {
            _geometryMap[id] = new GeometryResource(geometry, shape ?? new Shape(), size ?? geometrySize);
        }
        else
        {
            Debug.WriteLine($"Failed to load geometry resource '{id}'.");
        }
    }
    
    public GeometryResource Get(GeometryResourceId id)
    {
        if (id == GeometryResourceId.None)
        {
            return GeometryResource.Empty;
        }
        
        if (_geometryMap.TryGetValue(id, out var geometry))
        {
            return geometry;
        }

        return GeometryResource.Empty;
    }

    public DashStyle Get(LineStyleResourceId id)
    {
        if (_lineStyleMap.TryGetValue(id, out var style))
        {
            return style;
        }

        return DashStyles.Solid;
    }

    // NOTE: Throws a variety of exceptions on failure.
    private (Geometry, Shape?, Unit2D) LoadGeometry(string filename)
    {
        var schema = SchemaUtil.LoadProject(filename);

        Project project = new();

        ProjectSchema.Unpack(schema, project);

        var sheet = project.Sheets.First();

        var geometry = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        Shape? shape = null;
        UnitBounds? bounds = null;
        
        using (var ctx = geometry.Open())
        {
            shape = sheet.Elements.FirstOrDefault(e => e is Shape) as Shape;

            if (shape is not null)
            {
                var walker = new StreamGeometryWalker
                {
                    Context = ctx
                };
                
                foreach (var polygon in shape.PolygonSet)
                {
                    polygon.Transform(shape.Transform);
                    bounds = UnitBounds.Union(bounds, polygon.CalculateBounds());
                    
                    polygon.Resolver.Walk(walker);
                }

                shape.Transform = UnitTransform.Identity;
            }
        }

        geometry.Freeze();

        return (geometry, shape, bounds?.Size ?? Unit2D.Zero);
    }
}
