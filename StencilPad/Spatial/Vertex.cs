namespace StencilPad.Spatial;

public readonly record struct Vertex
{
    public Unit2D Position { get; init; }
    public CornerType CornerType { get; init; }
    public CornerSize CornerSize { get; init; }

    public Vertex(Unit2D position)
        : this(position, CornerType.None, CornerSize.Zero)
    { }

    public Vertex(Unit2D position, CornerType cornerType, CornerSize cornerSize)
    {
        Position = position;
        CornerType = cornerType;
        CornerSize = cornerSize;
    }
}
