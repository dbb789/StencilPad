using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Models;

public record GeometryResource
{
    public static readonly GeometryResource Empty = new GeometryResource(Geometry.Empty, UnitBounds.Empty);
    
    public Geometry Geometry { get; init; }
    public UnitBounds Bounds { get; init; }

    public GeometryResource(Geometry geometry,
                            UnitBounds bounds)
    {
        Geometry = geometry;
        Bounds = bounds;
    }
}
