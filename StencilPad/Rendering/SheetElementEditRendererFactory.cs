using StencilPad.Models;

namespace StencilPad.Rendering;

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

        if (element is ImageElement imageElement)
        {
            return new ImageElementEditRenderer(imageElement);
        }
        
        return null;
    }
}
