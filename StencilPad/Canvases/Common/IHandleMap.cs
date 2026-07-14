using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public interface IHandleMap
{
    int HandleCount { get; }
    ReadOnlyFlatSet<IHandleMapEntry> SelectedHandles { get; }

    event Action? SheetSelectionChanged;
    event Action<ISheetElement, Handle, Unit2D>? HandleAdded;
    event Action<ISheetElement, Handle>? HandleRemoved;
    event Action<ISheetElement, Handle, Unit2D>? HandleMoved;
    event Action? HandleSelectionChanged;

    void QueryHandles(UnitBounds bounds, List<IHandleMapEntry> results);
    HandleMapEntry? GetClosestEditingHandle(UnitBounds bounds);
    bool TryGetHandleEntry(Handle handle, out IHandleMapEntry entry);
    void SelectAll();
    void ClearSelection();
}
