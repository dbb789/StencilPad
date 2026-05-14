using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public interface IHandleMap
{
    event Action<IHandleSource, Handle, Unit2D>? HandleAdded;
    event Action<IHandleSource, Handle>? HandleRemoved;
    event Action<IHandleSource, Handle, Unit2D>? HandleMoved;
    
    void QueryHandles(UnitBounds bounds, List<(HandleMapEntry, Unit2D)> results);
}
