using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public interface IUnitSnap
{
    Unit2D? UnitSnap(Unit2D point,
                     IUnitSnapContext context);
}
