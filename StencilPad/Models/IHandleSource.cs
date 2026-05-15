using StencilPad.Spatial;

namespace StencilPad.Models;

public interface IHandleSource
{
    HandleSet Handles { get; }

    event Action<IHandleSource, Handle, Unit2D>? HandleAdded;
    event Action<IHandleSource, Handle>? HandleRemoved;
    event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    event Action<IHandleSource>? SelectionChanged;

    HandleSet GetSelectedHandles();
    void SetSelectedHandles(HandleSet handles);

    Unit2D GetPoint(Handle handle);
    void SetPoint(Handle handle, Unit2D position);
}
