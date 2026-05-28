using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class ShapeSchema : SheetElementSchema
{
    public PolygonSchema [] Polygons { get; set; } = [];
    public ColorSchema FillColor { get; set; } = new();
    public ColorSchema LineColor { get; set; } = new();
    public Unit LineWidth { get; set; } = new();
    public int StartCap { get; set; } = 0;
    public int EndCap { get; set; } = 0;
    
    public static ShapeSchema Pack(Shape shape)
    {
        return new ShapeSchema
        {
            Polygons = shape.PolygonSet.Select(p => PolygonSchema.Pack(p)).ToArray(),
            Transform = UnitTransformSchema.Pack(shape.Transform),
            FillColor = ColorSchema.Pack(shape.FillColor),
            LineColor = ColorSchema.Pack(shape.LineColor),
            LineWidth = shape.LineWidth,
            StartCap = shape.StartCap.ToValue(),
            EndCap = shape.EndCap.ToValue()
        };
    }

    public override Shape Unpack()
    {
        var shape = new Shape()
        {
            Transform = UnitTransformSchema.Unpack(Transform),
            FillColor = ColorSchema.Unpack(FillColor),
            LineColor = ColorSchema.Unpack(LineColor),
            LineWidth = LineWidth,
            StartCap = GeometryResourceId.FromValue(StartCap),
            EndCap = GeometryResourceId.FromValue(EndCap)
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
