using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public class DefaultUnitSnapContext : BaseUnitSnapContext
{
    public override IViewport Viewport { get; }

    public DefaultUnitSnapContext(IViewport viewport)
    {
        Viewport = viewport;
    }
}
