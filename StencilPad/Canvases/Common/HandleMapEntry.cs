using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public class HandleMapEntry
{
    public ISheetElement Element = null!;
    public IHandleSource Source => Element.HandleSource;
    public Handle Handle;
    public Unit2D Position;
    public bool ElementSelected;
    public bool HandleSelected;
}
