using StencilPad.Spatial;

namespace StencilPad.Schemas;

public class EdgeSchema
{
    public EdgeType Type { get; set; }
    public Unit2D? CtrlBegin { get; set; }
    public Unit2D? CtrlEnd { get; set; }

    public static EdgeSchema Pack(Edge edge)
    {
        return new EdgeSchema
        {
            Type = edge.Type,
            CtrlBegin = (edge.ControlBeginOffset != Unit2D.Zero) ? edge.ControlBeginOffset : null,
            CtrlEnd = (edge.ControlEndOffset != Unit2D.Zero) ? edge.ControlEndOffset : null
    };
    }

    public static Edge Unpack(EdgeSchema data)
    {
        return new Edge
        {
            Type = data.Type,
            ControlBeginOffset = data.CtrlBegin ?? Unit2D.Zero,
            ControlEndOffset = data.CtrlEnd ?? Unit2D.Zero
        };
    }
}
