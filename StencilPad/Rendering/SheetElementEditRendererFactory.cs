using StencilPad.Models;

namespace StencilPad.Rendering;

public static class SheetElementEditRendererFactory
{
    public static SheetElementEditRenderer? Create(ISheetElement element)
    {
        if (element is IPolygonSheetElement polygonElement)
        {
            return new PolygonEditRenderer(polygonElement);
        }

        if (element is TextElement textElement)
        {
            return new TextElementEditRenderer(textElement);
        }

        if (element is ImageElement imageElement)
        {
            return new ImageElementEditRenderer(imageElement);
        }

        if (element is ElementGroup elementGroup)
        {
            return new GroupEditRenderer(elementGroup);
        }
        
        return null;
    }
}
