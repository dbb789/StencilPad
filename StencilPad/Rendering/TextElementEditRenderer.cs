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
        _textElement.PropertyChanged += OnPropertyChanged;
    }

    public override void Dispose()
    {
        _textElement.GeometryChanged -= GeometryChanged;
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

        var transform = CreateTransform();
        dc.PushTransform(transform);
        dc.DrawRectangle(Brushes.Transparent, pen, rect);
        dc.Pop();
    }

    private Transform CreateTransform()
    {
        var group = new TransformGroup();
        if (_textElement.Transform.Angle != 0m)
        {
            group.Children.Add(new RotateTransform((double)_textElement.Transform.Angle));
        }
        group.Children.Add(new TranslateTransform(_textElement.Transform.Position.X.Millimeters,
                                                  _textElement.Transform.Position.Y.Millimeters));
        group.Freeze();
        return group;
    }

    private void GeometryChanged(ISheetElement _)
    {
        InvokeRendererDirty();
    }

    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TextElement.Transform))
        {
            InvokeRendererDirty();
        }
    }
}
