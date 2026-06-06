using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class MarkerPathSchema : SheetElementSchema
{
    public PolygonSchema Ply { get; set; } = new();
    public Unit Spc { get; set; } = Unit.FromMillimeters(2);
    public Unit Offs { get; set; } = Unit.FromMillimeters(4);
    public bool Bal { get; set; } = true;
    public int MkrT { get; set; } = 0;
    public Color MkrC { get; set; }
    public Color LnC { get; set; }
    public Unit LnW { get; set; } = new();
    
    public static MarkerPathSchema Pack(MarkerPath markerPath)
    {
        return new MarkerPathSchema
        {
            Ply = PolygonSchema.Pack(markerPath.Polygon),
            
            Spc = markerPath.Spacing,
            Offs = markerPath.Offset,
            Bal = markerPath.Balanced,
            MkrT = markerPath.MarkerType.ToValue(),
            MkrC = markerPath.MarkerColor,
            LnC = markerPath.LineColor,
            LnW = markerPath.LineWidth,
            Trns = UnitTransformSchema.Pack(markerPath.Transform)
        };
    }

    public override MarkerPath Unpack()
    {
        return new MarkerPath(PolygonSchema.Unpack(Ply))
        {
            Spacing = Spc,
            Offset = Offs,
            Balanced = Bal,
            MarkerType = GeometryResourceId.FromValue(MkrT),
            MarkerColor = MkrC,
            LineColor = LnC,
            LineWidth = LnW,
            Transform = UnitTransformSchema.Unpack(Trns)
        };
    }
}
