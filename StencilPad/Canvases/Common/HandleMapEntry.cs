using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public readonly record struct HandleMapEntry(ISheetElement Element,
                                             Handle Handle,
                                             Unit2D Position)
{
    public IHandleSource Source => Element.HandleSource;
}
