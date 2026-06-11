using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class EdgeSchema
{
    public int? T { get; set; }
    public Unit2D? CB { get; set; }
    public Unit2D? CE { get; set; }

    public static EdgeSchema Pack(Edge edge)
    {
        if (edge.Type == EdgeType.Straight)
        {
            return new EdgeSchema();
        }
        else
        {
            return new EdgeSchema
            {
                T = GetEdgeTypeId(edge.Type),
                CB = (edge.ControlBeginOffset != Unit2D.Zero) ? edge.ControlBeginOffset : null,
                CE = (edge.ControlEndOffset != Unit2D.Zero) ? edge.ControlEndOffset : null
            };
        }
    }

    public static Edge Unpack(EdgeSchema data)
    {
        return new Edge
        {
            Type = GetEdgeTypeFromId(data.T),
            ControlBeginOffset = data.CB ?? Unit2D.Zero,
            ControlEndOffset = data.CE ?? Unit2D.Zero
        };
    }

    public static int GetEdgeTypeId(EdgeType edgeType)
    {
        switch (edgeType)
        {
        case EdgeType.Straight:
            return 0;
            
        case EdgeType.Bezier:
            return 1;
            
        default:
            throw new ArgumentOutOfRangeException(nameof(edgeType), $"Unsupported edge type: {edgeType}");
        }
    }

    public static EdgeType GetEdgeTypeFromId(int? id)
    {
        if (!id.HasValue)
        {
            return EdgeType.Straight;
        }

        switch (id.Value)
        {
        case 0:
            return EdgeType.Straight;
            
        case 1:
            return EdgeType.Bezier;
            
        default:
            throw new ArgumentOutOfRangeException(nameof(id), $"Unsupported edge type ID: {id}");
        }
    }
}
