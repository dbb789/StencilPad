namespace StencilPad.Models;

public readonly record struct HandleSetId
{
    private readonly ulong _value;

    internal HandleSetId(ulong value)
    {
        _value = value;
    }
}
