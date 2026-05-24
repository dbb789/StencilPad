using System.IO;
using StencilPad.Rendering;

namespace StencilPad.Services;

public static class GeometryResourceLibrary
{
    private static readonly string ResourcesDirectory = "Resources";
    private static readonly string GeometryDirectory = Path.Combine(ResourcesDirectory, "Geometry");

    public static readonly GeometryResourceId None = new(0);
    public static readonly GeometryResourceId Arrow0 = new(1);

    public static readonly IList<GeometryResourceId> Resources = [ None, Arrow0 ];
    
    public static readonly IReadOnlyDictionary<GeometryResourceId, string> ResourceFiles =
        new Dictionary<GeometryResourceId, string>
        {
            { Arrow0, Path.Combine(GeometryDirectory, "Arrow0.spad") }
        };
}
