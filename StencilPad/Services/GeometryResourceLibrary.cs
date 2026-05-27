using System.IO;
using StencilPad.Models;

namespace StencilPad.Services;

public static class GeometryResourceLibrary
{
    private static readonly string ResourcesDirectory = "Resources";
    private static readonly string GeometryDirectory = Path.Combine(ResourcesDirectory, "Geometry");

    public static readonly IReadOnlyDictionary<GeometryResourceId, string> ResourceFiles =
        new Dictionary<GeometryResourceId, string>
        {
            { GeometryResourceId.Arrow0, Path.Combine(GeometryDirectory, "Arrow0.spad") }
        };
}
