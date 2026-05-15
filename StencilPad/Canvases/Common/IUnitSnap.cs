using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public interface IUnitSnap
{
    bool TryUnitSnap(Unit2D point, Handle? selfHandle, out Unit2D snappedPoint);
}
