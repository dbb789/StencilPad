namespace StencilPad.Models;

public record struct StartEndHandleKey : IHandleKey
{
    public enum EndType : byte
    {
        Start,
        End
    }

    public EndType Type { get; private set; }

    public HandleKeyType KeyType => HandleKeyType.StartEnd;
    
    public StartEndHandleKey(EndType type)
    {
        Type = type;
    }

    public ulong Pack()
    {
        return (ulong)Type;
    }

    public void Unpack(ulong key)
    {
        Type = (EndType)(key & 0xFF);
    }
}
