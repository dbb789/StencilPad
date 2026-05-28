using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class UnitTransformSchema
{
    public Unit2D Pos { get; set; } = Unit2D.Zero;
    public string Angle { get; set; } = "0";

    public static UnitTransformSchema Pack(UnitTransform transform)
    {
        return new UnitTransformSchema
        {
            Pos = transform.Position,
            Angle = transform.Angle.ToString()
        };
    }

    public static UnitTransform Unpack(UnitTransformSchema data)
    {
        if (!decimal.TryParse(data.Angle, out var angle))
        {
            angle = 0;
        }
        
        return new UnitTransform
        {
            Position = data.Pos,
            Angle = angle
        };
    }
}
