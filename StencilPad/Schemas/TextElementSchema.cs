using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class TextElementSchema : SheetElementSchema
{
    public Unit2D Min { get; set; } = Unit2D.Zero;
    public Unit2D Max { get; set; } = Unit2D.Zero;
    public string Text { get; set; } = "";
    public string Font { get; set; } = "Arial";
    public double FSz { get; set; } = 5.0;
    public Color Col { get; set; } = Color.FromArgb(255, 0, 0, 0);

    public static TextElementSchema Pack(TextElement element)
    {
        return new TextElementSchema
        {
            Min = element.Min,
            Max = element.Max,
            Text = element.Text,
            Font = element.FontName,
            FSz = element.FontSize,
            Col = element.Color,
            Trns = UnitTransformSchema.Pack(element.Transform)
        };
    }

    public override TextElement Unpack()
    {
        return new TextElement(Min, Text)
        {
            Max = Max,
            FontName = Font,
            FontSize = FSz,
            Color = Col,
            Transform = UnitTransformSchema.Unpack(Trns)
        };
    }
}
