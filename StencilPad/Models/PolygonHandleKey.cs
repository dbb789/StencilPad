namespace StencilPad.Models;

public record struct PolygonHandleKey : IHandleKey
{
    public static PolygonHandleKey Vertex(int index) => new(PolygonHandleType.Vertex, index);
    public static PolygonHandleKey ControlBegin(int index) => new(PolygonHandleType.ControlBegin, index);
    public static PolygonHandleKey ControlEnd(int index) => new(PolygonHandleType.ControlEnd, index);

    public HandleKeyType KeyType => HandleKeyType.Polygon;
    
    public PolygonHandleType Type { get; private set; }
    public int Index { get; private set; }

    public PolygonHandleKey(PolygonHandleType type, int index)
    {
        Type = type;
        Index = index;
    }

    public ulong Pack()
    {
        return ((ulong)Type << 32) | (uint)Index;
    }

    public void Unpack(ulong key)
    {
        Type = (PolygonHandleType)(key >> 32);
        Index = (int)(key & 0xFFFFFFFF);
    }
}
