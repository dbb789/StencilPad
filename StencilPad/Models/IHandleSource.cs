using StencilPad.Spatial;

namespace StencilPad.Models;

public interface IHandleSource
{
    event Action<IHandleSource, Handle, Unit2D, bool>? HandleAdded;
    event Action<IHandleSource, Handle>? HandleRemoved;
    event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    event Action<IHandleSource, Handle, bool>? HandleSelectionChanged;

    void QueryHandles(Action<Handle, Unit2D, bool> func);
    void SetHandleSelected(Handle handle, bool selected);
    
    Unit2D GetPoint(Handle handle);
    void SetPoint(Handle handle, Unit2D position);
}
