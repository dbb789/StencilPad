using StencilPad.Spatial;

namespace StencilPad.Models;

public interface IHandleSource<TSelf>
{
    event Action<TSelf, Handle, Unit2D, bool>? HandleAdded;
    event Action<TSelf, Handle>? HandleRemoved;
    event Action<TSelf, Handle, Unit2D>? HandleMoved;
    event Action<TSelf, Handle, bool>? HandleSelectionChanged;

    void QueryHandles(Action<Handle, Unit2D, bool> func);
    void SetHandleSelected(Handle handle, bool selected);
    
    Unit2D GetPoint(Handle handle);
    void SetPoint(Handle handle, Unit2D position);
}

public interface IHandleSource : IHandleSource<IHandleSource>
{
    // ...
}
