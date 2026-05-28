using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Models;

public record GeometryResource
{
    public static readonly GeometryResource Empty = new GeometryResource(Geometry.Empty, Unit2D.Zero);
    
    public Geometry Geometry { get; init; }
    public Unit2D Size { get; init; }

    public GeometryResource(Geometry geometry,
                            Unit2D size)
    {
        Geometry = geometry;
        Size = size;
    }
}
