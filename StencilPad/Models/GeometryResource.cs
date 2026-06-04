using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Models;

public record GeometryResource
{
    public static readonly GeometryResource Empty =
        new GeometryResource(Geometry.Empty, new Shape(), Unit2D.Zero);
    
    public Geometry Geometry { get; init; }
    public Shape Shape { get; init; }
    public Unit2D Size { get; init; }

    public GeometryResource(Geometry geometry,
                            Shape shape,
                            Unit2D size)
    {
        Geometry = geometry;
        Shape = shape;
        Size = size;
    }
}
