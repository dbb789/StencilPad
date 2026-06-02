using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class ShapeSchema : SheetElementSchema
{
    public PolygonSchema[] Plys { get; set; } = [];
    public Color FlC { get; set; }
    public Color LnC { get; set; }
    public Unit LnW { get; set; } = new();
    public int LnS { get; set; } = 0;
    public int StC { get; set; } = 0;
    public int EdC { get; set; } = 0;
    
    public static ShapeSchema Pack(Shape shape)
    {
        return new ShapeSchema
        {
            Plys = shape.PolygonSet.Select(p => PolygonSchema.Pack(p)).ToArray(),
            Trns = UnitTransformSchema.Pack(shape.Transform),
            FlC = shape.FillColor,
            LnC = shape.LineColor,
            LnW = shape.LineWidth,
            LnS = shape.LineStyle.ToValue(),
            StC = shape.StartCap.ToValue(),
            EdC = shape.EndCap.ToValue()
        };
    }

    public override Shape Unpack()
    {
        var shape = new Shape()
        {
            Transform = UnitTransformSchema.Unpack(Trns),
            FillColor = FlC,
            LineColor = LnC,
            LineWidth = LnW,
            LineStyle = LineStyleResourceId.FromValue(LnS),
            StartCap = GeometryResourceId.FromValue(StC),
            EndCap = GeometryResourceId.FromValue(EdC)
        };

        // The constructor adds one empty polygon, so clear it.
        shape.PolygonSet.Clear();

        foreach (var schema in Plys)
        {
            var editablePolygon = new EditablePolygon();

            editablePolygon.AssignFrom(PolygonSchema.Unpack(schema));
            shape.Add(editablePolygon);
        }

        return shape;
    }
}
