using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StencilPad.Rendering;
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

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _sheetRenderer.RendererDirty += ForceRedraw;
        _editOverlayRenderer.RendererDirty += ForceRedraw;
    }

    public void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _sheetRenderer.RendererDirty -= ForceRedraw;
        _editOverlayRenderer.RendererDirty -= ForceRedraw;
    }
    
    private void ForceRedraw()
    {
        InvalidateVisual();
    }
    
    protected override void OnRender(DrawingContext dc)
    {
        dc.PushTransform(_viewport.MillimetersToPixelsTransform);

        _sheetRenderer.Render(dc);
        _editOverlayRenderer.Render(dc);
        
        dc.Pop();
    }
}
