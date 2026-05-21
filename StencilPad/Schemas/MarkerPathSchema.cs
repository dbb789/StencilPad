using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class MarkerPathSchema : SheetElementSchema
{
    public PolygonSchema Polygon { get; set; } = new();
    public Unit Offset { get; set; } = Unit.FromMillimeters(4);
    public Unit Spacing { get; set; } = Unit.FromMillimeters(2);
    
    public static MarkerPathSchema Pack(MarkerPath markerPath)
    {
        return new MarkerPathSchema
        {
            Polygon = PolygonSchema.Pack(markerPath.Polygon),
            Offset = markerPath.Offset,
            Spacing = markerPath.Spacing,
            Transform = markerPath.Transform
        };
    }

    public override MarkerPath Unpack()
    {
        return new MarkerPath(PolygonSchema.Unpack(Polygon))
        {
            Offset = Offset,
            Spacing = Spacing,
            Transform = Transform
        };
    }
}
