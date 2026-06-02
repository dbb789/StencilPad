using System.ComponentModel;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class TextElementEditRenderer : SheetElementEditRenderer
{
    private readonly TextElement _textElement;

    public TextElementEditRenderer(TextElement textElement)
    {
        _textElement = textElement;
        _textElement.GeometryChanged += GeometryChanged;
        _textElement.WorldTransformChanged += OnWorldTransformChanged;
        _textElement.PropertyChanged += OnPropertyChanged;
    }

    public override void Dispose()
    {
        _textElement.GeometryChanged -= GeometryChanged;
        _textElement.WorldTransformChanged -= OnWorldTransformChanged;
        _textElement.PropertyChanged -= OnPropertyChanged;
    }

    public override void Render(DrawingContext dc)
    {
        var bounds = UnitBounds.FromMinMax(_textElement.Min, _textElement.Max);
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

        var transform = _textElement.WorldTransform.CreateGroupTransform();
        dc.PushTransform(transform);
        dc.DrawRectangle(Brushes.Transparent, pen, rect);
        dc.Pop();
    }

    private void OnWorldTransformChanged(ISheetElement _)
    {
        InvokeRendererDirty();
    }

    private void GeometryChanged(ISheetElement _)
    {
        InvokeRendererDirty();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvokeRendererDirty();
    }
}
