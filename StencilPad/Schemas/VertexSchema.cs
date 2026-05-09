using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class VertexSchema
{
    public Unit2D Pos { get; set; }
    public CornerType CornerType { get; set; }
    public Unit? CornerUnit { get; set; }
    public double? CornerProp { get; set; }

    public static VertexSchema Pack(Vertex vertex)
    {
        return new VertexSchema
        {
            Pos = vertex.Position,
            CornerType = vertex.CornerType,
            CornerUnit = vertex.CornerSize.IsUnit ? vertex.CornerSize.Unit : null,
            CornerProp = vertex.CornerSize.IsProportion ? vertex.CornerSize.Proportion : null
        };
    }

    public static Vertex Unpack(VertexSchema data)
    {
        var cornerSize = CornerSize.Zero;

        if (data.CornerUnit.HasValue)
        {
            cornerSize = CornerSize.FromUnit(data.CornerUnit.Value);
        }
        else if (data.CornerProp.HasValue)
        {
            cornerSize = CornerSize.FromProportion(data.CornerProp.Value);
        }
        
        return new Vertex(data.Pos, data.CornerType, cornerSize);
    }
}
