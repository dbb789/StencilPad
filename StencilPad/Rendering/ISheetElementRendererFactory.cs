using StencilPad.Models;

namespace StencilPad.Rendering;

public interface ISheetElementRendererFactory
{
    SheetElementRenderer? Create(ISheetElement element);
}
