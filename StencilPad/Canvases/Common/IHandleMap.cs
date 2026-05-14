using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public interface IHandleMap
{
    void QueryHandles(UnitBounds bounds, List<(Handle, Unit2D)> results);
}
