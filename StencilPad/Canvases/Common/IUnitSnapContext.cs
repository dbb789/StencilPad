using StencilPad.Models;

namespace StencilPad.Canvases.Common;

public interface IUnitSnapContext
{
    bool CanUnitSnapTo(ISheetElement element);
    bool CanUnitSnapTo(Handle handle);
}
