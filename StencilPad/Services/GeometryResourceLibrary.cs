using System.IO;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Services;

public static class GeometryResourceLibrary
{
    private static readonly string ResourcesDirectory = "Resources";
    private static readonly string GeometryDirectory = Path.Combine(ResourcesDirectory, "Geometry");

    public record Entry
    {
        public string Filename { get; init; }
        public Unit2D? Size { get; init; }

        public Entry(string filename, Unit2D? size = null)
        {
            Filename = Path.Combine(GeometryDirectory, filename);
            Size = size;
        }
    }
    
    public static readonly IReadOnlyDictionary<GeometryResourceId, Entry> ResourceFiles =
        new Dictionary<GeometryResourceId, Entry>
        {
            { GeometryResourceId.Arrow0, new Entry("Arrow0.spad") }
        };
}
