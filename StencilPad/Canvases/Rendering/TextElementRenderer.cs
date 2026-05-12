using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Rendering;

public class TextElementRenderer : SheetElementRenderer
{
    private static readonly FontFamily FallbackFont = new("Arial");

    public override TextElement Element => _textElement;

    public override UnitBounds SelectionBounds
    {
        get
        {
            if (_textElement.Size == Unit2D.Zero)
            {
                return UnitBounds.Empty;
            }

            return UnitBounds.FromMinMax(_textElement.Start, _textElement.End);
        }
    }

    private readonly TextElement _textElement;
    private FormattedText? _formattedText;

    public TextElementRenderer(TextElement textElement)
    {
        _textElement = textElement;
        _textElement.HandleSet.HandlesChanged += OnHandlesChanged;
        _textElement.PropertyChanged += OnPropertyChanged;
        RebuildFormattedText();
    }

    public override void Dispose()
    {
        _textElement.HandleSet.HandlesChanged -= OnHandlesChanged;
        _textElement.PropertyChanged -= OnPropertyChanged;
    }

    public override bool HitTest(Unit2D unit)
    {
        return SelectionBounds.Contains(unit);
    }

    public override bool BoundsTest(UnitBounds bounds)
    {
        return bounds.Contains(_textElement.Start);
    }

    public override void Render(DrawingContext dc)
    {
        if (_formattedText is null || string.IsNullOrEmpty(_textElement.Text))
        {
            return;
        }

        var size = _textElement.Size;
        var clipRect = new Rect(_textElement.Start.Millimeters,
                                new Size(size.X.Millimeters, size.Y.Millimeters));

        dc.PushClip(new RectangleGeometry(clipRect));
        dc.DrawText(_formattedText, _textElement.Start.Millimeters);
        dc.Pop();
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
            _textElement.FontSize,
            new SolidColorBrush(_textElement.Color),
            1.0)
        {
            Trimming = TextTrimming.None
        };

        var size = _textElement.Size;
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

    private void OnHandlesChanged()
    {
        RebuildFormattedText();
        InvokeInvalidateVisual();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        RebuildFormattedText();
        InvokeInvalidateVisual();
    }
}
