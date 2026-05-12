using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class ImageElementSchema : SheetElementSchema
{
    public Unit2D Start { get; set; } = Unit2D.Zero;
    public Unit2D End { get; set; } = Unit2D.Zero;
    // System.Text.Json serializes byte[] as a base64 string automatically.
    public byte[] ImageData { get; set; } = [];

    public static ImageElementSchema Pack(ImageElement element)
    {
        return new ImageElementSchema
        {
            Start = element.Start,
            End = element.End,
            ImageData = element.ImageData
        };
    }

    public override ImageElement Unpack()
    {
        return new ImageElement(Start, End, ImageData);
    }
}
