using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Rendering;
using StencilPad.Schemas;
using StencilPad.Spatial;

namespace StencilPad.Services;

public class ResourceService : IResourceService
{
    private Dictionary<GeometryResourceId, GeometryResource> _geometryCache;
    
    public ResourceService()
    {
        _geometryCache = [];

        Load();
    }

    private void Load()
    {
        foreach (var (id, entry) in GeometryResourceLibrary.ResourceFiles)
        {
            Load(id, entry.Filename, entry.Size);
        }        
    }

    private void Load(GeometryResourceId id, string filename, Unit2D? size)
    {
        Geometry? geometry = null;
        Unit2D geometrySize = Unit2D.Zero;
        
        try
        {
            
            (geometry, geometrySize) = LoadGeometry(filename);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error loading geometry resource '{id}' from file '{filename}': {e.Message}");
            return;
        }
        
        if (geometry != null)
        {
            _geometryCache[id] = new GeometryResource(geometry, size ?? geometrySize);
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
        
        if (_geometryCache.TryGetValue(id, out var geometry))
        {
            return geometry;
        }

        return GeometryResource.Empty;
    }

    public DashStyle Get(LineStyleResourceId id)
    {
        if (LineStyleResourceLibrary.ResourceMap.TryGetValue(id, out var style))
        {
            return style;
        }

        return DashStyles.Solid;
    }

    // NOTE: Throws a variety of exceptions on failure.
    private (Geometry?, Unit2D) LoadGeometry(string filename)
    {
        var schema = SchemaUtil.LoadProject(filename);

        Project project = new();

        ProjectSchema.Unpack(schema, project);

        var sheet = project.Sheets.First();

        var geometry = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        UnitBounds? bounds = null;
        
        using (var ctx = geometry.Open())
        {
            foreach (var element in sheet.Elements)
            {
                if (element is Shape shape)
                {
                    var walker = new StreamGeometryWalker
                    {
                        Context = ctx
                    };
                    
                    foreach (var polygon in shape.PolygonSet)
                    {
                        polygon.Transform(shape.Transform);
                        bounds = UnitBounds.Union(bounds, polygon.CalculateBounds());

                        polygon.Resolver.WalkPolygon(walker);
                    }
                }
            }
        }

        geometry.Freeze();

        return (geometry, bounds?.Size ?? Unit2D.Zero);
    }
}
