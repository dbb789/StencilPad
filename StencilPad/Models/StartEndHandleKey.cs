namespace StencilPad.Models;

public readonly record struct StartEndHandleKey : IComparable<StartEndHandleKey>
{
    public enum EndType : byte
    {
        Start,
        End
    }

    public EndType Type { get; init; }

    public StartEndHandleKey(EndType type)
    {
        Type = type;
    }

    public int CompareTo(StartEndHandleKey other)
    {
        return Type.CompareTo(other.Type);
    }
}
