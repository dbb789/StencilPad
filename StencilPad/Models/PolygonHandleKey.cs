namespace StencilPad.Models;

public readonly record struct PolygonHandleKey : IComparable<PolygonHandleKey>
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

    public int CompareTo(PolygonHandleKey other)
    {
        var cmp = Type.CompareTo(other.Type);

        if (cmp != 0)
        {
            return cmp;
        }
        
        return Index.CompareTo(other.Index);
    }
}
