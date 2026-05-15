using System.Collections;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class HandleSet : IEnumerable<Handle>
{
    public int Count => Handles.Count;
    public Handle this[int index] => Handles[index];
    
    protected readonly FlatSet<Handle> Handles;
    
    public HandleSet(int initialCapacity = 0)
    {
        Handles = new FlatSet<Handle>(initialCapacity);
    }

    protected HandleSet(HandleSet other)
    {
        Handles = new FlatSet<Handle>(other.Handles);
    }
    
    protected HandleSet(FlatSet<Handle> handles)
    {
        Handles = handles;
    }

    public bool Contains(Handle handle)
    {
        return Handles.Contains(handle);
    }
    
    public FlatSet<Handle>.Enumerator GetEnumerator()
    {
        return Handles.GetEnumerator();
    }

    IEnumerator<Handle> IEnumerable<Handle>.GetEnumerator()
    {
        return Handles.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Handles.GetEnumerator();
    }

    public static HandleSet Intersection(HandleSet a, HandleSet b)
    {
        return new HandleSet(FlatSet<Handle>.Intersection(a.Handles, b.Handles));
    }
}
