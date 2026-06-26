using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class ImageElementSchema : SheetElementSchema
{
    public Unit2D Min { get; set; } = Unit2D.Zero;
    public Unit2D Max { get; set; } = Unit2D.Zero;
    
    // System.Text.Json serializes byte[] as a base64 string automatically.
    public byte[] Data { get; set; } = [];
    public double? Opac { get; set; }

    public static ImageElementSchema Pack(ImageElement element)
    {
        return new ImageElementSchema
        {
            Min = element.Min,
            Max = element.Max,
            Data = element.ImageData,
            Opac = (Math.Abs(element.Opacity - 1.0) < 1e-6) ? null : element.Opacity,
            Trns = UnitTransformSchema.Pack(element.Transform)
        };
    }

    public override ImageElement Unpack()
    {
        return new ImageElement(Min, Max, Data)
        {
            Opacity = Opac ?? 1.0,
            Transform = UnitTransformSchema.Unpack(Trns)
        };
    }
}
