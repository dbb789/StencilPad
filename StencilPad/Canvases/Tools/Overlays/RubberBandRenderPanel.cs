using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StencilPad.Canvases.Tools.Overlays;

public class RubberBandRenderPanel : ContentControl
{
    private static readonly Brush RubberBandFill;
    private static readonly Pen RubberBandBorder;

    static RubberBandRenderPanel()
    {
        RubberBandFill = new SolidColorBrush(Color.FromArgb(40, 0, 120, 215));
        RubberBandFill.Freeze();
        RubberBandBorder = new Pen(new SolidColorBrush(Color.FromArgb(200, 0, 120, 215)), 1.0);
        RubberBandBorder.Freeze();
    }

    private Rect? _dragRegion;

    public void Updated(Rect? dragRegion)
    {
        _dragRegion = dragRegion;
        
        InvalidateVisual();
    }
    
    protected override void OnRender(DrawingContext dc)
    {
        if (_dragRegion is null)
        {
            return;
        }

        dc.DrawRectangle(RubberBandFill, RubberBandBorder, _dragRegion.Value);
    }
}
