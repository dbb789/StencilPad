namespace StencilPad.Spatial;

public class MutableAssignableList<T> : AssignableList<T>
{
    // Directly exposed so that list can be manipulated by the containing class
    // without needing to invoke an event for every change.
    public List<T> Items => _items;
    
    public MutableAssignableList(int capacity = 0)
        : base(capacity)
    { }
}
