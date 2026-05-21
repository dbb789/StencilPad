using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class ImageElementSchema : SheetElementSchema
{
    public Unit2D Min { get; set; } = Unit2D.Zero;
    public Unit2D Max { get; set; } = Unit2D.Zero;
    
    // System.Text.Json serializes byte[] as a base64 string automatically.
    public byte[] ImageData { get; set; } = [];

    public static ImageElementSchema Pack(ImageElement element)
    {
        return new ImageElementSchema
        {
            Min = element.Min,
            Max = element.Max,
            ImageData = element.ImageData,
            Transform = element.Transform
        };
    }

    public override ImageElement Unpack()
    {
        return new ImageElement(Min, Max, ImageData)
        {
            Transform = Transform
        };
    }
}
