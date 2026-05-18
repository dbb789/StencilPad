namespace StencilPad.Spatial;

public class KeyedList<T> : IKeyedList<T>
{
    public T this[int index]
    {
        get => _data[index].Item1;
        set
        {
            var oldValue = _data[index].Item1;

            if (!EqualityComparer<T>.Default.Equals(oldValue, value))
            {
                _data[index] = (value, _data[index].Item2);
                ItemReassigned?.Invoke(index, _data[index].Item2, oldValue, value);
            }
        }
    }
    
    public int Count => _data.Count;

    private List<(T, ulong)> _data;
    private FlatMap<ulong, int> _indices;
    private ulong _counter;
    
    public event Action<int, ulong, T, T>? ItemReassigned;

    public KeyedList(int initialCapacity = 0)
    {
        _data = new List<(T, ulong)>(initialCapacity);
        _indices = new FlatMap<ulong, int>(initialCapacity);
        _counter = 0;
    }

    public void Add(T value)
    {
        var key = ++_counter;

        _data.Add((value, key));
        _indices.Add(key, _data.Count - 1);
    }

    public void AddRange(IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            Add(value);
        }
    }

    public void Insert(int index, T value)
    {
        var key = ++_counter;

        _data.Insert(index, (value, key));
        
        for (int i = index + 1; i < _data.Count; i++)
        {
            var cycleKey = _data[i].Item2;

            _indices[cycleKey] = i;
        }
        
        _indices.Add(key, index);
    }

    public void RemoveAt(int index)
    {
        var key = _data[index].Item2;
        
        _data.RemoveAt(index);
        _indices.Remove(key);

        for (int i = index; i < _data.Count; i++)
        {
            var cycleKey = _data[i].Item2;

            _indices[cycleKey] = i;
        }
    }
    
    public void Clear()
    {
        _data.Clear();
        _indices.Clear();
    }

    public T At(int index)
    {
        index %= _data.Count;

        if (index < 0)
        {
            index += _data.Count;
        }
        
        return _data[index].Item1;
    }
    
    public ulong KeyAt(int index)
    {
        index %= _data.Count;

        if (index < 0)
        {
            index += _data.Count;
        }
        
        return _data[index].Item2;
    }
    
    public int IndexOfKey(ulong key)
    {
        return _indices[key];
    }

    public T GetByKey(ulong key)
    {
        return _data[_indices[key]].Item1;
    }
    
    public void AssignFrom(KeyedList<T> other)
    {
        _data.Clear();
        _data.AddRange(other._data);
        _indices.AssignFrom(other._indices);
        _counter = other._counter;
    }

    public KeyedList<T> DeepClone()
    {
        var clone = new KeyedList<T>();

        clone._data = new(_data);
        clone._indices = new(_indices);
        clone._counter = _counter;

        return clone;
    }
}
