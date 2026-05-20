using StencilPad.Models;

namespace StencilPad.Canvases.Rendering;

public interface ISheetElementRendererFactory
{
    SheetElementRenderer? Create(ISheetElement element);
}
