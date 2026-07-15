namespace StencilPad.Collections;

public class FlatSet<T> : ReadOnlyFlatSet<T>
{
	public FlatSet(int initialCapacity = 0)
        : base(initialCapacity)
	{ }
    
    public FlatSet(ReadOnlyFlatSet<T> other)
        : base(other)
    { }

	public bool Add(T element)
	{
		var index = Array.BinarySearch(_data, 0, _dataLength, element);

		if (index >= 0)
		{
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
		_data[elementIndex] = element;
		
		return true;
	}

    public void AddRange(IEnumerable<T> elements)
    {
        foreach (var element in elements)
        {
            Add(element);
        }
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

    public void AssignFrom(FlatSet<T> other)
    {
        if (_data.Length < other._dataLength)
        {
            _data = new T[other._dataLength];
        }

        Array.Copy(other._data, _data, other._dataLength);
        _dataLength = other._dataLength;
    }
    
	private void ResizeArray()
	{
		Array.Resize(ref _data, Math.Max(4, _data.Length * 2));
	}

	public void Clear()
	{
		_dataLength = 0;
	}
}
