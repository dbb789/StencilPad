using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public interface IHandleMap
{
    ReadOnlyFlatSet<IHandleMapEntry> SelectedHandles { get; }

    event Action? SheetSelectionChanged;
    event Action<IHandleSource, Handle, Unit2D>? HandleAdded;
    event Action<IHandleSource, Handle>? HandleRemoved;
    event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    event Action? HandleSelectionChanged;

    void QueryHandles(UnitBounds bounds, List<IHandleMapEntry> results);
    HandleMapEntry? GetClosestHandle(UnitBounds bounds);
    bool TryGetHandleEntry(Handle handle, out IHandleMapEntry entry);
    void ClearSelection();
}
