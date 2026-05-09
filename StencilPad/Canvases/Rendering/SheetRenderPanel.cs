using System.Windows.Controls;
using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Rendering;

public class SheetRenderPanel : ContentControl
{
    private SheetRenderer _sheetRenderer;
    private IViewport _viewport;

    public SheetRenderPanel(SheetRenderer sheetRenderer, IViewport viewport)
    {
        _sheetRenderer = sheetRenderer;
        _viewport = viewport;

        _sheetRenderer.InvalidateVisual += InvalidateVisual;
    }

    protected override void OnRender(DrawingContext dc)
    {
        dc.PushTransform(_viewport.GetMillimetersToPixelsTransform());

        for (int i = 0; i < _sheetRenderer.Count; ++i)
        {
            _sheetRenderer[i].Render(dc);
        }

        dc.Pop();
    }
}
