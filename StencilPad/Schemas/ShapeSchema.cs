using StencilPad.Models;

namespace StencilPad.Schemas;

public class ShapeSchema : SheetElementSchema
{
    public PolygonSchema Polygon { get; set; } = new();

    public static ShapeSchema Pack(Shape shape)
    {
        return new ShapeSchema
        {
            Polygon = PolygonSchema.Pack(shape.EditablePolygon)
        };
    }

    public override Shape Unpack()
    {
        return new Shape(PolygonSchema.Unpack(Polygon));
    }
}
