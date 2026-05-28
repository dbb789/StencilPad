namespace StencilPad.Schemas;

public class GeometryResourceLibrarySchema
{
    public List<GeometryResourceSchema> Caps { get; set; } = [];
    public List<GeometryResourceSchema> Markers { get; set; } = [];
}
