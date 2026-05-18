namespace StencilPad.Models;

public record struct PolygonHandleKey : IHandleKey
{
    public static PolygonHandleKey Vertex(ulong key) => new(PolygonHandleType.Vertex, key);
    public static PolygonHandleKey ControlBegin(ulong key) => new(PolygonHandleType.ControlBegin, key);
    public static PolygonHandleKey ControlEnd(ulong key) => new(PolygonHandleType.ControlEnd, key);

    public HandleKeyType KeyType => HandleKeyType.Polygon;
    
    public PolygonHandleType Type { get; private set; }
    public ulong Key { get; private set; }

    public PolygonHandleKey(PolygonHandleType type, ulong key)
    {
        Type = type;
        Key = key;
    }

    public ulong Pack()
    {
        return ((ulong)Type << 60) | (Key & 0xFFFFFFFFFFFFFFF);
    }

    public void Unpack(ulong key)
    {
        Type = (PolygonHandleType)(key >> 60);
        Key = (key & 0xFFFFFFFFFFFFFFF);
    }
}
