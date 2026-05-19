namespace StencilPad.Models;

public record struct MinMaxHandleKey : IHandleKey
{
    public enum HandleType : byte
    {
        Min,
        Max
    }

    public HandleKeyType KeyType => HandleKeyType.MinMax;
    public HandleType Type { get; private set; }

    
    public MinMaxHandleKey(HandleType type)
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
