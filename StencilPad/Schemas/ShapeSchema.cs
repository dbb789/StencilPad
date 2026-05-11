using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class ShapeSchema : SheetElementSchema
{
    public PolygonSchema Polygon { get; set; } = new();
    public Color FillColor { get; set; } = new();
    public Color LineColor { get; set; } = new();
    public Unit LineWidth { get; set; } = new();
    
    public static ShapeSchema Pack(Shape shape)
    {
        return new ShapeSchema
        {
            Polygon = PolygonSchema.Pack(shape.EditablePolygon),
            FillColor = shape.FillColor,
            LineColor = shape.LineColor,
            LineWidth = shape.LineWidth
        };
    }

    public override Shape Unpack()
    {
        return new Shape(PolygonSchema.Unpack(Polygon))
        {
            FillColor = FillColor,
            LineColor = LineColor,
            LineWidth = LineWidth
        };
    }
}
