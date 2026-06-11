using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class VertexSchema
{
    public Unit2D Pos { get; set; }
    public int? CT { get; set; }
    public Unit? CU { get; set; }
    public double? CP { get; set; }

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
                CT = GetCornerTypeId(vertex.CornerType),
                CU = vertex.CornerSize.IsUnit ? vertex.CornerSize.Unit : null,
                CP = vertex.CornerSize.IsProportion ? vertex.CornerSize.Proportion : null
            };
        }
    }

    public static Vertex Unpack(VertexSchema data)
    {
        var cornerType = GetCornerTypeFromId(data.CT);
        var cornerSize = CornerSize.Zero;
        
        if (data.CU.HasValue)
        {
            cornerSize = CornerSize.FromUnit(data.CU.Value);
        }
        else if (data.CP.HasValue)
        {
            cornerSize = CornerSize.FromProportion(data.CP.Value);
        }
        
        return new Vertex(data.Pos, cornerType, cornerSize);
    }

    public static int? GetCornerTypeId(CornerType cornerType)
    {
        switch (cornerType)
        {
        case CornerType.None:
            return null;
            
        case CornerType.Rounded:
            return 1;
            
        case CornerType.Beveled:
            return 2;
            
        default:
            throw new InvalidOperationException($"Invalid corner type: {cornerType}");
        }
    }

    public static CornerType GetCornerTypeFromId(int? id)
    {
        if (!id.HasValue)
        {
            return CornerType.None;
        }

        switch (id.Value)
        {
        case 1:
            return CornerType.Rounded;
            
        case 2:
            return CornerType.Beveled;
            
        default:
            throw new InvalidOperationException($"Invalid corner type ID: {id}");
        }
    }
}
