using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class RulerSchema : SheetElementSchema
{
    public Unit2D Min { get; set; } = Unit2D.Zero;
    public Unit2D Max { get; set; } = Unit2D.Zero;
    public string Font { get; set; } = "Arial";
    public double FSz { get; set; } = 8.0;
    public Color Col { get; set; } = Color.FromArgb(255, 0, 0, 0);

    public static RulerSchema Pack(Ruler ruler)
    {
        return new RulerSchema
        {
            Min = ruler.Min,
            Max = ruler.Max,
            Font = ruler.FontName,
            FSz = ruler.FontSize,
            Col = ruler.Color,
            Trns = UnitTransformSchema.Pack(ruler.Transform)
        };
    }

    public override Ruler Unpack()
    {
        return new Ruler
        {
            Min = Min,
            Max = Max,
            FontName = Font,
            FontSize = FSz,
            Color = Col,
            Transform = UnitTransformSchema.Unpack(Trns)
        };
    }
}
