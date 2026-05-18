using StencilPad.Models;

namespace StencilPad.Canvases.Common;

public interface IUnitSnapContext
{
    bool CanUnitSnapTo(IHandleSource source);
    bool CanUnitSnapTo(Handle handle);
}
