namespace StencilPad.Spatial;

public class AssignableList<T>
{
    private static readonly EqualityComparer<T> EqualityComparer = EqualityComparer<T>.Default;
    
    public struct Enumerator
    {
        private readonly List<T> _items;
        private int _index;

        public T Current => _items[_index];

        public Enumerator(List<T> items)
        {
            _items = items;
            _index = -1;
        }

        public bool MoveNext()
        {
            return ++_index < _items.Count;
        }
    }

    public int Count => _items.Count;
    
    protected List<T> _items;
    
    public event Action<int, T, T>? ItemReassigned;
    
    public AssignableList(int capacity = 0)
    {
        _items = new List<T>(capacity);
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(_items);
    }

    public T At(int index)
    {
        index %= _items.Count;

        if (index < 0)
        {
            index += _items.Count;
        }
        
        return _items[index];
    }
    
    public T this[int index]
    {
        get => _items[index];
        set
        {
            if (EqualityComparer.Equals(_items[index], value))
            {
                return;
            }

            var oldValue = _items[index];
            
            _items[index] = value;

            ItemReassigned?.Invoke(index, oldValue, value);
        }
    }
}
