using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Rendering;

public class ImageElementEditRenderer : SheetElementEditRenderer
{
    private readonly ImageElement _imageElement;

    public ImageElementEditRenderer(ImageElement imageElement)
    {
        _imageElement = imageElement;
        _imageElement.HandleSet.HandlesChanged += OnChanged;
    }

    public override void Dispose()
    {
        _imageElement.HandleSet.HandlesChanged -= OnChanged;
    }

    public override void Render(DrawingContext dc)
    {
        var bounds = UnitBounds.FromMinMax(_imageElement.Start, _imageElement.End);
        if (bounds.Size == Unit2D.Zero)
        {
            return;
        }

        var pen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)), 0.2)
        {
            DashStyle = DashStyles.Dot
        };

        dc.DrawRectangle(Brushes.Transparent, pen, bounds.Millimeters);
    }

    private void OnChanged() => InvokeInvalidateVisual();
}
