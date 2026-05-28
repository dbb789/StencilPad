using System.Windows.Media;

namespace StencilPad.Schemas;

public class ColorSchema
{
    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }
    public int A { get; set; }

    public static ColorSchema Pack(Color color)
    {
        return new ColorSchema
        {
            R = color.R,
            G = color.G,
            B = color.B,
            A = color.A
        };
    }

    public static Color Unpack(ColorSchema data)
    {
        return new Color
        {
            R = (byte)data.R,
            G = (byte)data.G,
            B = (byte)data.B,
            A = (byte)data.A
        };
    }
}
