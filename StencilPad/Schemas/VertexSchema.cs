using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class VertexSchema
{
    public Unit2D Pos { get; set; }
    public CornerType? Corner { get; set; }
    public Unit? CornerUnit { get; set; }
    public double? CornerProp { get; set; }

    public static VertexSchema Pack(Vertex vertex)
    {
        if (vertex.CornerType == CornerType.None)
        {
            return new VertexSchema
            {
                Pos = vertex.Position
            };
        }
        else
        {
            return new VertexSchema
            {
                Pos = vertex.Position,
                Corner = vertex.CornerType,
                CornerUnit = vertex.CornerSize.IsUnit ? vertex.CornerSize.Unit : null,
                CornerProp = vertex.CornerSize.IsProportion ? vertex.CornerSize.Proportion : null
            };
        }
    }

    public static Vertex Unpack(VertexSchema data)
    {
        var cornerType = data.Corner ?? CornerType.None;
        var cornerSize = CornerSize.Zero;
        
        if (data.CornerUnit.HasValue)
        {
            cornerSize = CornerSize.FromUnit(data.CornerUnit.Value);
        }
        else if (data.CornerProp.HasValue)
        {
            cornerSize = CornerSize.FromProportion(data.CornerProp.Value);
        }
        
        return new Vertex(data.Pos, cornerType, cornerSize);
    }
}
