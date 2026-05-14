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

    public void Add(Handle handle)
    {
        Handles.Add(handle);
    }

    public void Remove(Handle handle)
    {
        Handles.Remove(handle);
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
