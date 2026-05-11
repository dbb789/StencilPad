using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class RulerSchema : SheetElementSchema
{
    public Unit2D Start { get; set; } = Unit2D.Zero;
    public Unit2D End { get; set; } = Unit2D.Zero;
    
    public static RulerSchema Pack(Ruler ruler)
    {
        return new RulerSchema
        {
            Start = ruler.Start,
            End = ruler.End
        };
    }

    public override Ruler Unpack()
    {
        return new Ruler
        {
            Start = Start,
            End = End
        };
    }
}
