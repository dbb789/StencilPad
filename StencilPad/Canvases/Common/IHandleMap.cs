using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public interface IHandleMap
{
    event Action<IHandleSource, Handle, Unit2D>? HandleAdded;
    event Action<IHandleSource, Handle>? HandleRemoved;
    event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    event Action? HandleSelectionChanged;

    void QueryHandles(UnitBounds bounds, List<HandleMapEntry> results);
    void QuerySelectedElementHandles(UnitBounds bounds, List<HandleMapEntry> results);
}
