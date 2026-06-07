using StencilPad.Spatial;
using StencilPad.Models;

namespace StencilPad.Canvases.Common;

public abstract class BaseUnitSnapContext : IUnitSnapContext
{
    public abstract IViewport Viewport { get; }
    
    public virtual bool CanUnitSnapTo(ISheetElement element)
    {
        return true;
    }

    public virtual bool CanUnitSnapTo(Handle handle)
    {
        return true;
    }
}
