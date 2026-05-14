namespace StencilPad.Models;

public readonly record struct HandleSourceId
{
    private readonly ulong _value;

    public HandleSourceId(ulong value)
    {
        _value = value;
    }
}
