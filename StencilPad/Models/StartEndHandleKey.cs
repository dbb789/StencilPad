namespace StencilPad.Models;

public readonly record struct StartEndHandleKey : IHandleKey
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
}
