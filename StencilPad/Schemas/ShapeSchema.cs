using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class ShapeSchema : SheetElementSchema
{
    public PolygonSchema [] Polygons { get; set; } = [];
    public UnitTransform Transform { get; set; } = UnitTransform.Identity;
    public Color FillColor { get; set; } = new();
    public Color LineColor { get; set; } = new();
    public Unit LineWidth { get; set; } = new();
    
    public static ShapeSchema Pack(Shape shape)
    {
        return new ShapeSchema
        {
            Polygons = shape.PolygonSet.Select(p => PolygonSchema.Pack(p)).ToArray(),
            Transform = shape.Transform,
            FillColor = shape.FillColor,
            LineColor = shape.LineColor,
            LineWidth = shape.LineWidth
        };
    }

    public override Shape Unpack()
    {
        var shape = new Shape()
        {
            Transform = Transform,
            FillColor = FillColor,
            LineColor = LineColor,
            LineWidth = LineWidth
        };

        // The constructor adds one empty polygon, so clear it.
        shape.PolygonSet.Clear();

        foreach (var schema in Polygons)
        {
            var editablePolygon = new EditablePolygon();

            editablePolygon.AssignFrom(PolygonSchema.Unpack(schema));
            shape.Add(editablePolygon);
        }

        return shape;
    }
}
