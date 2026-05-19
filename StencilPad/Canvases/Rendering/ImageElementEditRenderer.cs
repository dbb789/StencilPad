using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Rendering;

public class ImageElementEditRenderer : SheetElementEditRenderer
{
    private static Pen OutlinePen;

    static ImageElementEditRenderer()
    {
        OutlinePen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)), 0.2)
        {
            DashStyle = DashStyles.Dot
        };
        
        OutlinePen.Freeze();
    }
    
    private readonly ImageElement _imageElement;

    public ImageElementEditRenderer(ImageElement imageElement)
    {
        _imageElement = imageElement;
        _imageElement.GeometryChanged += InvokeInvalidateVisual;
    }

    public override void Dispose()
    {
        _imageElement.GeometryChanged -= InvokeInvalidateVisual;
    }

    public override void Render(DrawingContext dc)
    {
        var bounds = UnitBounds.FromMinMax(_imageElement.Min, _imageElement.Max);
        
        if (bounds.Size == Unit2D.Zero)
        {
            return;
        }

        dc.DrawRectangle(Brushes.Transparent, OutlinePen, bounds.Millimeters);
    }
}
