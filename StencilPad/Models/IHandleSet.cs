namespace StencilPad.Models;

public interface IHandleSet
{
    IEnumerable<Handle> Handles { get; }
    
    event Action? HandlesChanged;
    event Action? SelectionChanged;

    IEnumerable<Handle> GetSelectedHandles();
    void SetSelectedHandles(IEnumerable<Handle> handles);
}
