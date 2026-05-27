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
        foreach (var (id, path) in GeometryResourceLibrary.ResourceFiles)
        {
            Load(id, path);
        }        
    }

    private void Load(GeometryResourceId id, string filename)
    {
        Geometry? geometry = null;
        UnitBounds bounds;
        
        try
        {
            (geometry, bounds) = LoadGeometry(filename);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error loading geometry resource '{id}' from file '{filename}': {e.Message}");
            return;
        }
        
        if (geometry != null)
        {
            _geometryCache[id] = new GeometryResource(geometry, bounds);
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
    private (Geometry?, UnitBounds) LoadGeometry(string filename)
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
                    bounds = UnitBounds.Union(bounds, shape.GetBounds(UnitTransform.Identity));
                    ShapeRenderer.AddToGeometry(shape, ctx);
                }
            }
        }

        geometry.Freeze();

        return (geometry, bounds ?? UnitBounds.Empty);
    }
}
