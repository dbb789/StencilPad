namespace StencilPad.Models;

public readonly record struct HandleSourceId : IComparable<HandleSourceId>
{
    private readonly ulong _value;

    public HandleSourceId(ulong value)
    {
        _value = value;
    }

    public int CompareTo(HandleSourceId other)
    {
        return _value.CompareTo(other._value);
    }
}
