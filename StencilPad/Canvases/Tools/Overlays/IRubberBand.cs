using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public interface IRubberBand
{
    bool IsActive { get; set; }

    event Action<UnitBounds>? BoundsSelected;
    event Action<Unit2D>? PointSelected;
}
