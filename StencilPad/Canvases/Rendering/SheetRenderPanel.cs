using System.Windows.Controls;
using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Rendering;

public class SheetRenderPanel : ContentControl
{
    private SheetRenderer _sheetRenderer;
    private EditOverlayRenderer _editOverlayRenderer;
    private IViewport _viewport;

    public SheetRenderPanel(SheetRenderer sheetRenderer,
                            EditOverlayRenderer editOverlayRenderer,
                            IViewport viewport)
    {
        _sheetRenderer = sheetRenderer;
        _editOverlayRenderer = editOverlayRenderer;
        _viewport = viewport;

        _sheetRenderer.InvalidateVisual += InvalidateVisual;
        _editOverlayRenderer.InvalidateVisual += InvalidateVisual;
    }

    protected override void OnRender(DrawingContext dc)
    {
        dc.PushTransform(_viewport.GetMillimetersToPixelsTransform());

        _sheetRenderer.Render(dc);
        _editOverlayRenderer.Render(dc);
        
        dc.Pop();
    }
}
