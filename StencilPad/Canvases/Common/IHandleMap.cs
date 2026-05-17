using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public interface IHandleMap
{
    IEnumerable<HandleMapEntry> SelectedHandles { get; }

    event Action? SheetSelectionChanged;
    event Action<IHandleSource, Handle, Unit2D>? HandleAdded;
    event Action<IHandleSource, Handle>? HandleRemoved;
    event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    event Action? HandleSelectionChanged;

    void QueryHandles(UnitBounds bounds, List<HandleMapEntry> results);
    HandleMapEntry? GetClosestHandle(UnitBounds bounds);
    bool TryGetHandleEntry(Handle handle, out HandleMapEntry entry);
    void ClearSelection();
}
