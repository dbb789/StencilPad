using StencilPad.Models;

namespace StencilPad.Canvases.Rendering;

public static class SheetElementRendererFactory
{
    public static SheetElementRenderer? Create(ISheetElement element)
    {
        SheetElementRenderer? renderer = null;
        
        if (element is Shape shape)
        {
            renderer = new ShapeRenderer(shape);
        }
        else if (element is MarkerPath markerPath)
        {
            renderer = new MarkerPathRenderer(markerPath);
        }
        else if (element is Ruler ruler)
        {
            renderer = new RulerRenderer(ruler);
        }

        return renderer;
    }
}
