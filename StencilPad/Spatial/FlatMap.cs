using System.Collections;

namespace StencilPad.Spatial;

public class FlatMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
{
    private class KeyComparer<TCKey, TCValue> : IComparer<KeyValuePair<TCKey, TCValue>>
    {
        private readonly IComparer<TCKey> _keyComparer;

        public KeyComparer(IComparer<TCKey> keyComparer)
        {
            _keyComparer = keyComparer;
        }

        public int Compare(KeyValuePair<TCKey, TCValue> x, KeyValuePair<TCKey, TCValue> y)
        {
            return _keyComparer.Compare(x.Key, y.Key);
        }
    }    

    private static readonly KeyComparer<TKey, TValue> Comparer = new KeyComparer<TKey, TValue>(Comparer<TKey>.Default);
    
	public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
	{
        public KeyValuePair<TKey, TValue> Current => _data[_index];
        KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current => _data[_index];
        object? IEnumerator.Current => _data[_index];
        
		private KeyValuePair<TKey, TValue> [] _data;
		private int _dataLength;
		private int _index;
		
		public Enumerator(KeyValuePair<TKey, TValue> [] data, int dataLength)
		{
			_data = data;
			_dataLength = dataLength;
			_index = -1;
		}

		public bool MoveNext()
		{
			return ++_index < _dataLength;
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
	
	private KeyValuePair<TKey, TValue> [] _data;
	private int _dataLength;

	public TValue this[TKey key]
	{
		get => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();
        set => Add(key, value);
	}
	
	public int Count => _dataLength;

	public FlatMap(int initialCapacity = 0)
	{
		_data = new KeyValuePair<TKey, TValue>[initialCapacity];
		_dataLength = 0;
	}
    
    public FlatMap(FlatMap<TKey, TValue> other)
    {
        _data = new KeyValuePair<TKey, TValue>[other._data.Length];
        Array.Copy(other._data, _data, other._dataLength);
        _dataLength = other._dataLength;
    }

	public bool Add(TKey key, TValue value)
	{
        var kvp = new KeyValuePair<TKey, TValue>(key, value);
		var index = Array.BinarySearch(_data, 0, _dataLength, kvp, Comparer);

		if (index >= 0)
		{
			_data[index] = kvp;
            
            return false;
		}

		var elementIndex = ~index;

		if (_dataLength >= _data.Length)
		{
			ResizeArray();
		}

		var count = _dataLength - elementIndex;

		if (count > 0)
		{
			Array.Copy(_data, elementIndex, _data, elementIndex + 1, count);
		}
		
		++_dataLength;
        _data[elementIndex] = kvp;

        return true;
	}
	
	public bool Remove(TKey key)
	{
        var kvp = new KeyValuePair<TKey, TValue>(key, default!);
		var index = Array.BinarySearch(_data, 0, _dataLength, kvp, Comparer);
		
		if (index < 0)
		{
			return false;
		}

        RemoveAt(index);

		return true;
	}

    public void RemoveAt(int index)
    {
        var count = (_dataLength - index) - 1;

        if (count > 0)
        {
            Array.Copy(_data, index + 1, _data, index, count);
        }
        
        --_dataLength;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        var kvp = new KeyValuePair<TKey, TValue>(key, default!);
        var index = Array.BinarySearch(_data, 0, _dataLength, kvp, Comparer);

        if (index >= 0)
        {
            value = _data[index].Value;

            return true;
        }

        value = default!;

        return false;
    }

	private void ResizeArray()
	{
		Array.Resize(ref _data, Math.Max(4, _data.Length * 2));
	}

	public void Clear()
	{
		_dataLength = 0;
	}

	public bool Contains(TKey key)
	{
        var kvp = new KeyValuePair<TKey, TValue>(key, default!);
        
		return Array.BinarySearch(_data, 0, _dataLength, kvp, Comparer) >= 0;
	}
        
	public Enumerator GetEnumerator()
	{
		return new Enumerator(_data, _dataLength);
	}

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return new Enumerator(_data, _dataLength);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(_data, _dataLength);
    }
}
