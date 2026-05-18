namespace StencilPad.Models;

public readonly struct Handle : IEquatable<Handle>, IComparable<Handle>
{
    public static readonly Handle DisplayOnly = new(default, HandleType.Move, 0, 0);

    public HandleSourceId HandleSetId { get; init; }
    public HandleType Type { get; init; }

    private readonly HandleKeyType _keyType;
    private readonly ulong _key;

    public bool CanGroupMove => Type == HandleType.Move;

    public static Handle Move<TKey>(HandleSourceId handleSetId, TKey key) where TKey : IHandleKey, new()
    {
        return new(handleSetId, HandleType.Move, key.KeyType, key.Pack());
    }
    
    public static Handle Adjust<TKey>(HandleSourceId handleSetId, TKey key) where TKey : IHandleKey, new()
    {
        return new(handleSetId, HandleType.Adjust, key.KeyType, key.Pack());
    }

    private Handle(HandleSourceId handleSetId, HandleType type, HandleKeyType keyType, ulong key)
    {
        HandleSetId = handleSetId;
        Type = type;
        
        _keyType = keyType;
        _key = key;
    }

    public TKey GetKey<TKey>() where TKey : IHandleKey, new()
    {
        var key = new TKey();

        if (key.KeyType != _keyType)
        {
            throw new InvalidOperationException($"Handle key type mismatch. Expected {key.KeyType}, got {_keyType}.");
        }

        key.Unpack(_key);

        return key;
    }
    
    public bool Equals(Handle other)
    {
        return HandleSetId == other.HandleSetId &&
            Type == other.Type &&
            _keyType == other._keyType &&
            _key == other._key;
    }
    
    public override bool Equals(object? obj)
    {
        return obj is Handle h && Equals(h);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(HandleSetId, Type, _keyType, _key);
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

        cmp = _keyType.CompareTo(other._keyType);

        if (cmp != 0)
        {
            return cmp;
        }

        return _key.CompareTo(other._key);
    }

    public override string ToString()
    {
        return $"[{HandleSetId}, {Type}, {_keyType}, {_key}]";
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
