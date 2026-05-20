namespace StencilPad.Spatial;

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
        int count = elements.Count();
        
        if (_data.Length < _dataLength + count)
        {
            var data = new T[_dataLength + count];
            
            Array.Copy(_data, data, _dataLength);

            _data = data;
        }

        foreach (var element in elements)
        {
            Add(element);
        }
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
