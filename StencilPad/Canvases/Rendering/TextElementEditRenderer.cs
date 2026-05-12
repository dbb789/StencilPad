using System.Windows;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Rendering;

public class TextElementEditRenderer : SheetElementEditRenderer
{
    private readonly TextElement _textElement;

    public TextElementEditRenderer(TextElement textElement)
    {
        _textElement = textElement;
        _textElement.HandleSet.HandlesChanged += OnChanged;
    }

    public override void Dispose()
    {
        _textElement.HandleSet.HandlesChanged -= OnChanged;
    }

    public override void Render(DrawingContext dc)
    {
        var bounds = UnitBounds.FromMinMax(_textElement.Start, _textElement.End);
        var size = bounds.Size;

        if (size == Unit2D.Zero)
        {
            return;
        }

        var rect = bounds.Millimeters;

        var pen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)), 0.2)
        {
            DashStyle = DashStyles.Dot
        };

        dc.DrawRectangle(Brushes.Transparent, pen, rect);
    }

    private void OnChanged()
    {
        InvokeInvalidateVisual();
    }
}
