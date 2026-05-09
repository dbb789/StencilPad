namespace StencilPad.Spatial;

public record struct Edge
{
    public EdgeType Type { get; init; }
    public Unit2D ControlBeginOffset { get; init; }
    public Unit2D ControlEndOffset { get; init; }

    public Edge()
    {
        Type = EdgeType.Straight;
        ControlBeginOffset = Unit2D.Zero;
        ControlEndOffset = Unit2D.Zero;
    }

    public Edge(Unit2D controlBeginOffset, Unit2D controlEndOffset)
    {
        Type = EdgeType.Bezier;
        ControlBeginOffset = controlBeginOffset;
        ControlEndOffset = controlEndOffset;
    }
}
