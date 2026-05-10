namespace StencilPad.Models;

public readonly record struct PolygonHandleKey : IHandleKey
{
    public static PolygonHandleKey Vertex(int index) => new(PolygonHandleType.Vertex, index);
    public static PolygonHandleKey ControlBegin(int index) => new(PolygonHandleType.ControlBegin, index);
    public static PolygonHandleKey ControlEnd(int index) => new(PolygonHandleType.ControlEnd, index);
    
    public PolygonHandleType Type { get; init; }
    public int Index { get; init; }

    public PolygonHandleKey(PolygonHandleType type, int index)
    {
        Type = type;
        Index = index;
    }
}
