namespace StencilPad.Models;

public readonly record struct Handle
{
    public static Handle Move(IHandleKey Key) => new(Key, HandleType.Move);
    public static Handle Adjust(IHandleKey Key) => new(Key, HandleType.Adjust);

    public bool CanGroupMove => Type == HandleType.Move;

    private readonly IHandleKey _key;
    public HandleType Type { get; init; }
    
    public Handle(IHandleKey key, HandleType type)
    {
        _key = key;
        Type = type;
    }

    public TKey Key<TKey>() where TKey : IHandleKey
    {
        if (_key is not TKey)
        {
            throw new InvalidOperationException($"Handle key is of type {_key.GetType().Name}, not {typeof(TKey).Name}");
        }
        
        return (TKey)_key;
    }
}
