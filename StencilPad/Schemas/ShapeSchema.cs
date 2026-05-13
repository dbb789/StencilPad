using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class ShapeSchema : SheetElementSchema
{
    public PolygonSchema [] Polygons { get; set; } = [];
    public Unit2D Position { get; set; } = Unit2D.Zero;
    public Color FillColor { get; set; } = new();
    public Color LineColor { get; set; } = new();
    public Unit LineWidth { get; set; } = new();
    
    public static ShapeSchema Pack(Shape shape)
    {
        return new ShapeSchema
        {
            Polygons = shape.PolygonSet.Select(p => PolygonSchema.Pack(p)).ToArray(),
            Position = shape.Position,
            FillColor = shape.FillColor,
            LineColor = shape.LineColor,
            LineWidth = shape.LineWidth
        };
    }

    public override Shape Unpack()
    {
        var shape = new Shape()
        {
            Position = Position,
            FillColor = FillColor,
            LineColor = LineColor,
            LineWidth = LineWidth
        };

        foreach (var schema in Polygons)
        {
            var editablePolygon = new EditablePolygon();

            editablePolygon.AssignFrom(PolygonSchema.Unpack(schema));
            shape.Add(editablePolygon);
        }

        return shape;
    }
}
