namespace StencilPad.Models;

public readonly struct Handle : IEquatable<Handle>, IComparable<Handle>
{
    public static readonly Handle DisplayOnly = new(default, HandleType.Move, HandleKey.None);

    public HandleSourceId HandleSetId { get; init; }
    public HandleType Type { get; init; }
    public HandleKey Key { get; init; }
    
    public bool CanGroupMove => Type == HandleType.Move;

    public static Handle Move(HandleSourceId handleSetId, PolygonHandleKey key) =>
        new(handleSetId, HandleType.Move, new HandleKey(key));

    public static Handle Adjust(HandleSourceId handleSetId, PolygonHandleKey key) =>
        new(handleSetId, HandleType.Adjust, new HandleKey(key));

    public static Handle Move(HandleSourceId handleSetId, StartEndHandleKey key) =>
        new(handleSetId, HandleType.Move, new HandleKey(key));

    private Handle(HandleSourceId handleSetId, HandleType type, HandleKey key)
    {
        HandleSetId = handleSetId;
        Type = type;
        Key = key;
    }

    public bool Equals(Handle other)
    {
        return HandleSetId == other.HandleSetId &&
            Type == other.Type &&
            Key == other.Key;
    }
    
    public override bool Equals(object? obj)
    {
        return obj is Handle h && Equals(h);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(HandleSetId, Type, Key);
    }

    public int CompareTo(Handle other)
    {
        int cmp = HandleSetId.CompareTo(other.HandleSetId);
        
        if (cmp != 0)
        {
            return cmp;
        }

        cmp = Type.CompareTo(other.Type);
        
        if (cmp != 0)
        {
            return cmp;
        }

        return Key.CompareTo(other.Key);
    }
    
    public static bool operator==(Handle lhs, Handle rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator!=(Handle lhs, Handle rhs)
    {
        return !(lhs == rhs);
    }
}
