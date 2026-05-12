using StencilPad.Models;

namespace StencilPad.Canvases.Rendering;

public static class SheetElementEditRendererFactory
{
    public static SheetElementEditRenderer? Create(ISheetElement element)
    {
        if (element is Shape shape)
        {
            return new ShapeEditRenderer(shape);
        }

        if (element is TextElement textElement)
        {
            return new TextElementEditRenderer(textElement);
        }
        
        return null;
    }
}
