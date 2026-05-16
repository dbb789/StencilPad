using StencilPad.Spatial;

namespace StencilPad.Models;

public class MutableHandleSet : HandleSet
{
    public new Handle this[int index]
    {
        get => Handles[index];
        set => Handles[index] = value;
    }

    public MutableHandleSet(int initialCapacity = 0)
        : base(initialCapacity)
    {
        // ...
    }

    public MutableHandleSet(HandleSet other)
        : base(other)
    {
        // ...
    }
    
    protected MutableHandleSet(FlatSet<Handle> handles)
        : base(handles)
    {
        // ...
    }

    public bool Add(Handle handle)
    {
        return Handles.Add(handle);
    }

    public bool Remove(Handle handle)
    {
        return Handles.Remove(handle);
    }

    public void RemoveAt(int index)
    {
        Handles.RemoveAt(index);
    }

    public void AddRange(IEnumerable<Handle> handles)
    {
        Handles.AddRange(handles);
    }

    public void Clear()
    {
        Handles.Clear();
    }
}
