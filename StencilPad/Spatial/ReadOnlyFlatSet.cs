using System.Collections;

namespace StencilPad.Spatial;

public class ReadOnlyFlatSet<T> : IEnumerable<T>
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

    protected IComparer<T>? _comparer;
	protected T [] _data;
	protected int _dataLength;

	public T this[int index]
	{
		get => _data[index];
	}
	
	public int Count => _dataLength;

	protected ReadOnlyFlatSet(int initialCapacity)
	{
		_data = new T[initialCapacity];
		_dataLength = 0;
	}
    
    public ReadOnlyFlatSet(ReadOnlyFlatSet<T> other)
    {
        _data = new T[other._data.Length];
        Array.Copy(other._data, _data, other._dataLength);
        _dataLength = other._dataLength;
    }

	public bool Contains(T element)
	{
		return Array.BinarySearch(_data, 0, _dataLength, element, _comparer) >= 0;
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
