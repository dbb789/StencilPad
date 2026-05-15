using System.Collections;

namespace StencilPad.Spatial;

public class FlatSet<T> : IEnumerable<T> where T : struct
{
	public struct Enumerator : IEnumerator<T>
	{
        public T Current => _data[_index];
        T IEnumerator<T>.Current => _data[_index];
        object? IEnumerator.Current => _data[_index];
        
		private T [] _data;
		private int _dataLength;
		private int _index;
		
		public Enumerator(T [] data, int dataLength)
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
	
	private T [] _data;
	private int _dataLength;

	public T this[int index]
	{
		get => _data[index];
		set => _data[index] = value;
	}
	
	public int Count => _dataLength;

	public FlatSet(int initialCapacity = 0)
	{
		_data = new T[initialCapacity];
		_dataLength = 0;
	}
    
    public FlatSet(FlatSet<T> other)
    {
        _data = new T[other._data.Length];
        Array.Copy(other._data, _data, other._dataLength);
        _dataLength = other._dataLength;
    }

	public bool Add(T element)
	{
		var index = Array.BinarySearch(_data, 0, _dataLength, element);

		if (index >= 0)
		{
			return false;
		}

		var elementIndex = ~index;

		if (_dataLength >= _data.Length - 1)
		{
			ResizeArray();
		}

		var count = _dataLength - elementIndex;

		if (count > 0)
		{
			Array.Copy(_data, elementIndex, _data, elementIndex + 1, count);
		}
		
		++_dataLength;
		_data[elementIndex] = element;
		
		return true;
	}
	
	public bool Remove(T element)
	{
		var index = Array.BinarySearch(_data, 0, _dataLength, element);
		
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

    public void AddRange(IEnumerable<T> elements)
    {
        if (_data.Length < _dataLength + elements.Count())
        {
            _data = new T[_dataLength + elements.Count()];
            Array.Copy(_data, _data, _dataLength);
        }

        foreach (var element in elements)
        {
            _data[_dataLength++] = element;
        }

        Array.Sort(_data, 0, _dataLength);
    }

    public static FlatSet<T> Intersection(FlatSet<T> a, FlatSet<T> b)
    {
        var result = new FlatSet<T>(a.Count + b.Count);

        for (int i = 0, j = 0; i < a.Count && j < b.Count;)
        {
            var comparison = Comparer<T>.Default.Compare(a[i], b[j]);

            if (comparison == 0)
            {
                result.Add(a[i]);
                i++;
                j++;
            }
            else if (comparison < 0)
            {
                i++;
            }
            else
            {
                j++;
            }
        }

        return result;
    }

	private void ResizeArray()
	{
		Array.Resize(ref _data, Math.Max(4, _data.Length * 2));
	}

	public void Clear()
	{
		_dataLength = 0;
	}

	public bool Contains(T element)
	{
		return Array.BinarySearch(_data, 0, _dataLength, element) >= 0;
	}
        
	public Enumerator GetEnumerator()
	{
		return new Enumerator(_data, _dataLength);
	}

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new Enumerator(_data, _dataLength);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(_data, _dataLength);
    }
}
