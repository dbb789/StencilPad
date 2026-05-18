using StencilPad.Models;

namespace StencilPad.Canvases.Common;

public abstract class BaseUnitSnapContext : IUnitSnapContext
{
    public virtual bool CanUnitSnapTo(IHandleSource source)
    {
        return true;
    }

    public virtual bool CanUnitSnapTo(Handle handle)
    {
        return true;
    }
}
