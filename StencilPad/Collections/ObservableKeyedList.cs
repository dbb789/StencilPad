using System.Collections;

namespace StencilPad.Collections;

public class ObservableKeyedList<TKey, TValue> : IEnumerable<TValue>, IObservableList<TValue>
    where TKey : notnull
{
    public struct Enumerator : IEnumerator<TValue>
    {
        public TValue Current => _parent[_index];
        object? IEnumerator.Current => _parent[_index];

        private readonly ObservableKeyedList<TKey, TValue> _parent;
        private readonly int _version;
        private int _index;
        
        public Enumerator(ObservableKeyedList<TKey, TValue> parent)
        {
            _parent = parent;
            _version = _parent._version;
            _index = -1;
        }

        public bool MoveNext()
        {
            if (_parent is null)
            {
                return false;
            }

            if (_version != _parent._version)
            {
                throw new InvalidOperationException("Collection was modified during enumeration.");
            }

            return ++_index < _parent.Count;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose()
        {
            // ..
        }
    }

    public TValue this[int index] =>_collection.GetAt(index).Value;
    public int Count => _collection.Count;

    private readonly OrderedDictionary<TKey, TValue> _collection;
    private int _version;
 
    // NOTE: Strictly defined to be called before CollectionChanged.
    public event Action<TValue>? ElementRemoving;
    
    public event Action<ObservableListChangedArgs<TValue>>? ListChanged;

    public ObservableKeyedList()
    {
        _collection = new();
        _version = 0;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return _collection.TryGetValue(key, out value!);
    }
    
    public void Add(TKey key, TValue value)
    {
        _collection.Add(key, value);
        ++_version;

        ListChanged?.Invoke(ObservableListChangedArgs<TValue>.Add(value, _collection.Count - 1));
    }

    public bool Remove(TKey key)
    {
        if (_collection.TryGetValue(key, out var value))
        {
            ElementRemoving?.Invoke(value);
            _collection.Remove(key);
            ++_version;

            ListChanged?.Invoke(ObservableListChangedArgs<TValue>.Remove(value));

            return true;
        }

        return false;
    }

    public void Insert(int index, TKey key, TValue value)
    {
        _collection.Insert(index, key, value);
        ++_version;

        ListChanged?.Invoke(ObservableListChangedArgs<TValue>.Add(value, index));
    }

    public void Move(int oldIndex, int newIndex)
    {
        var kvp = _collection.GetAt(oldIndex);

        _collection.RemoveAt(oldIndex);
        _collection.Insert(newIndex, kvp.Key, kvp.Value);

        ListChanged?.Invoke(ObservableListChangedArgs<TValue>.Move(kvp.Value, oldIndex, newIndex));
    }

    public void Clear()
    {
        foreach (var key in _collection.Keys.ToList())
        {
            Remove(key);
        }
    }

    public int IndexOf(TValue value)
    {
        for (int i = 0; i < _collection.Count; ++i)
        {
            if (EqualityComparer<TValue>.Default.Equals(_collection.GetAt(i).Value, value))
            {
                return i;
            }
        }

        return -1;
    }
    
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(this);
    }
}
