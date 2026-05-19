using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class RulerSchema : SheetElementSchema
{
    public Unit2D Min { get; set; } = Unit2D.Zero;
    public Unit2D Max { get; set; } = Unit2D.Zero;
    
    public static RulerSchema Pack(Ruler ruler)
    {
        return new RulerSchema
        {
            Min = ruler.Min,
            Max = ruler.Max
        };
    }

    public override Ruler Unpack()
    {
        return new Ruler
        {
            Min = Min,
            Max = Max
        };
    }
}
