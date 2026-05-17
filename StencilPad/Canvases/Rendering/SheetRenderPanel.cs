using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Rendering;

public class SheetRenderPanel : ContentControl
{
    private SheetRenderer _sheetRenderer;
    private EditOverlayRenderer _editOverlayRenderer;
    private IViewport _viewport;
    private bool _redrawPending;

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
        _sheetRenderer.InvalidateVisual += ForceRedraw;
        _editOverlayRenderer.InvalidateVisual += ForceRedraw;

        CompositionTarget.Rendering += OnRendering;
    }

    public void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _sheetRenderer.InvalidateVisual -= ForceRedraw;
        _editOverlayRenderer.InvalidateVisual -= ForceRedraw;

        CompositionTarget.Rendering -= OnRendering;
    }
    
    private void ForceRedraw()
    {
        _redrawPending = true;
    }
    
    private void OnRendering(object? sender, EventArgs e)
    {
        if (_redrawPending)
        {
            InvalidateVisual();
            _redrawPending = false;
        }
    }

    protected override void OnRender(DrawingContext dc)
    {
        dc.PushTransform(_viewport.GetMillimetersToPixelsTransform());

        _sheetRenderer.Render(dc);
        _editOverlayRenderer.Render(dc);
        
        dc.Pop();
    }
}
