using StencilPad.Spatial;
using StencilPad.Models;

namespace StencilPad.Canvases.Common;

public interface IUnitSnapContext
{
    IViewport Viewport { get; }
    
    bool CanUnitSnapTo(ISheetElement element);
    bool CanUnitSnapTo(Handle handle);
}
