using System.Collections;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class HandleSet : IEnumerable<Handle>
{
    public int Count => _handles.Count;
    public Handle this[int index]
    {
        get => _handles[index];
    }
    
    private readonly FlatSet<Handle> _handles;

    public HandleSet(int initialCapacity = 0)
    {
        _handles = new FlatSet<Handle>(initialCapacity);
    }

    public HandleSet(HandleSet other)
    {
        _handles = new FlatSet<Handle>(other._handles);
    }
    
    public bool Add(Handle handle)
    {
        return _handles.Add(handle);
    }

    public bool Remove(Handle handle)
    {
        return _handles.Remove(handle);
    }

    public void RemoveAt(int index)
    {
        _handles.RemoveAt(index);
    }

    public void Clear()
    {
        _handles.Clear();
    }

    public bool Contains(Handle handle)
    {
        return _handles.Contains(handle);
    }
    
    public void AssignFrom(HandleSet other)
    {
        _handles.AssignFrom(other._handles);
    }

    public FlatSet<Handle>.Enumerator GetEnumerator()
    {
        return _handles.GetEnumerator();
    }

    IEnumerator<Handle> IEnumerable<Handle>.GetEnumerator()
    {
        return _handles.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _handles.GetEnumerator();
    }
}
