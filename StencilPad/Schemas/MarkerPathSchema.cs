using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class MarkerPathSchema : SheetElementSchema
{
    public PolygonSchema Ply { get; set; } = new();
    public Unit Offs { get; set; } = Unit.FromMillimeters(4);
    public Unit Spc { get; set; } = Unit.FromMillimeters(2);
    
    public static MarkerPathSchema Pack(MarkerPath markerPath)
    {
        return new MarkerPathSchema
        {
            Ply = PolygonSchema.Pack(markerPath.Polygon),
            Offs = markerPath.Offset,
            Spc = markerPath.Spacing,
            Trns = UnitTransformSchema.Pack(markerPath.Transform)
        };
    }

    public override MarkerPath Unpack()
    {
        return new MarkerPath(PolygonSchema.Unpack(Ply))
        {
            Offset = Offs,
            Spacing = Spc,
            Transform = UnitTransformSchema.Unpack(Trns)
        };
    }
}
