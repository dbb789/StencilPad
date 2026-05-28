using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class GeometryResourceSchema
{
    public int Id { get; set; }
    public string Filename { get; set; } = "";
    public Unit2D? Size { get; set; }
}
