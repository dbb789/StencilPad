using StencilPad.Spatial;

namespace StencilPad.Models;

public interface IHandleSet
{
    IEnumerable<Handle> Handles { get; }
    
    event Action? HandlesChanged;
    event Action? SelectionChanged;

    IEnumerable<Handle> GetSelectedHandles();
    void SetSelectedHandles(IEnumerable<Handle> handles);

    Unit2D GetPoint(Handle handle);
    void SetPoint(Handle handle, Unit2D position);
}
