namespace StencilPad.Models;

public record struct BoundsHandleKey : IHandleKey
{
    public enum HandleType : byte
    {
        NW,
        NE,
        SW,
        SE
    }
    
    public HandleKeyType KeyType => HandleKeyType.Bounds;
    public HandleType Type { get; private set; }
    
    public BoundsHandleKey(HandleType type)
    {
        Type = type;
    }

    public ulong Pack()
    {
        return (ulong)Type;
    }

    public void Unpack(ulong key)
    {
        Type = (HandleType)(key & 0xFF);
    }
}
