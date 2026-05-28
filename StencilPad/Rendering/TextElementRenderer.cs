using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class TextElementRenderer : SheetElementRenderer
{
    private static readonly FontFamily FallbackFont = new("Arial");

    public override TextElement Element => _textElement;

    private readonly TextElement _textElement;
    private FormattedText? _formattedText;

    public TextElementRenderer(TextElement textElement)
    {
        _textElement = textElement;
        _textElement.GeometryChanged += GeometryChanged;
        _textElement.TransformChanged += TransformChanged;
        _textElement.PropertyChanged += OnPropertyChanged;
        
        RebuildFormattedText();
    }

    public override void Dispose()
    {
        _textElement.GeometryChanged -= GeometryChanged;
        _textElement.TransformChanged -= TransformChanged;
        _textElement.PropertyChanged -= OnPropertyChanged;
    }

    public override void Render(DrawingContext dc)
    {
        if (_formattedText is null || string.IsNullOrEmpty(_textElement.Text))
        {
            return;
        }

        var bounds = UnitBounds.FromMinMax(_textElement.Min, _textElement.Max);
        var clipRect = bounds.Millimeters;

        dc.PushTransform(_textElement.Transform.CreateGroupTransform());
        dc.PushClip(new RectangleGeometry(clipRect));
        dc.DrawText(_formattedText, clipRect.TopLeft);
        dc.Pop();
        dc.Pop();
    }
    
    private void TransformChanged(ISheetElement element)
    {
        InvokeRendererDirty();
    }

    private void RebuildFormattedText()
    {
        if (string.IsNullOrEmpty(_textElement.Text))
        {
            _formattedText = null;
            return;
        }

        var fontFamily = ResolveFont(_textElement.FontName);

        _formattedText = new FormattedText(
            _textElement.Text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            Unit.FromFontSizePoints(_textElement.FontSize).Millimeters,
            new SolidColorBrush(_textElement.Color),
            1.0)
        {
            Trimming = TextTrimming.None
        };

        var size = UnitBounds.FromMinMax(_textElement.Min, _textElement.Max).Size;
        if (size.X.Millimeters > 0)
        {
            _formattedText.MaxTextWidth = size.X.Millimeters;
        }
        if (size.Y.Millimeters > 0)
        {
            _formattedText.MaxTextHeight = size.Y.Millimeters;
        }
    }

    private static FontFamily ResolveFont(string fontName)
    {
        if (Fonts.SystemFontFamilies.Any(f => string.Equals(f.Source, fontName, StringComparison.OrdinalIgnoreCase)))
        {
            return new FontFamily(fontName);
        }

        return FallbackFont;
    }

    public static Unit2D Measure(TextElement textElement)
    {
        if (string.IsNullOrEmpty(textElement.Text))
        {
            return Unit2D.Zero;
        }

        var fontFamily = ResolveFont(textElement.FontName);

        var ft = new FormattedText(
            textElement.Text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            textElement.FontSize,
            Brushes.Black,
            1.0);

        return new Unit2D(Unit.FromMillimeters(ft.Width + 0.5), Unit.FromMillimeters(ft.Height));
    }

    private void GeometryChanged(ISheetElement _)
    {
        RebuildFormattedText();
        InvokeRendererDirty();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        RebuildFormattedText();
        InvokeRendererDirty();
    }
}
