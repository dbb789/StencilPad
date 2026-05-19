using StencilPad.Canvases.Common;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public interface IUnitSnapOverlay
{
    Unit2D? LastSnapPoint { get; }

    void Begin(IUnitSnapContext context);
    void End();
}
