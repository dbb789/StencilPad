using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using StencilPad.Canvases.Rendering;
using StencilPad.Models;
using StencilPad.Schemas;

namespace StencilPad.Services;

public class ResourceService : IResourceService
{
    private Dictionary<GeometryResourceId, Geometry> _geometryCache;
    private Geometry _placeholderGeometry;
    
    public ResourceService()
    {
        _geometryCache = [];
        _placeholderGeometry = CreatePlaceholderGeometry();

        Load();
    }

    private void Load()
    {
        Load(GeometryResourceId.Arrow0, "Resources/Arrow0.spad");
    }

    private void Load(GeometryResourceId id, string filename)
    {
        Geometry? geometry = null;

        try
        {
            geometry = LoadGeometry(filename);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error loading geometry resource '{id}' from file '{filename}': {e.Message}");
        }

        if (geometry != null)
        {
            _geometryCache[id] = geometry;
        }
        else
        {
            Debug.WriteLine($"Failed to load geometry resource '{id}'.");
        }
    }
    
    public Geometry Get(GeometryResourceId id)
    {
        if (_geometryCache.TryGetValue(id, out var geometry))
        {
            return geometry;
        }

        return _placeholderGeometry;
    }

     // NOTE: Throws a variety of exceptions on failure.
    private Geometry? LoadGeometry(string filename)
    {
        var schema = SchemaUtil.LoadProject(filename);

        Project project = new();

        ProjectSchema.Unpack(schema, project);

        var sheet = project.Sheets.First();

        var geometry = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        using (var ctx = geometry.Open())
        {
            foreach (var element in sheet.Elements)
            {
                if (element is Shape shape)
                {
                    ShapeRenderer.AddToGeometry(shape, ctx);
                }
            }
        }

        geometry.Freeze();

        return geometry;
    }

    private Geometry CreatePlaceholderGeometry()
    {
        var geometry = new EllipseGeometry(new Point(0, 0), 10, 10);

        geometry.Freeze();

        return geometry;
    }
}
