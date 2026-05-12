using System.Windows;
using System.Windows.Media;
using StencilPad.Models;

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
        var start = _textElement.Start.Millimeters;
        var size = _textElement.Size;

        if (size == Spatial.Unit2D.Zero)
        {
            return;
        }

        var rect = new Rect(start, new Size(size.X.Millimeters, size.Y.Millimeters));

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
