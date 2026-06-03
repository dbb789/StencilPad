using System.Globalization;
using System.Windows;
using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Models.Resolvers;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public class TextRenderer : ITextWalker, IWalkerRenderer
{
    private static readonly FontFamily FallbackFont = new("Arial");

    private Transform _transform;
    private TextStyle _style;
    private UnitBounds _bounds;
    private string _text;
    private FormattedText? _formattedText;

    public event Action? RendererDirty;
    
    public TextRenderer()
    {
        _transform = Transform.Identity;
        _style = new TextStyle();
        _text = "";
        
    }

    public void Dispose()
    {
        // ...
    }

    public void SetTransform(UnitTransform transform)
    {
        _transform = transform.CreateGroupTransform();
        InvokeRendererDirty();
    }

    public void SetStyle(TextStyle style)
    {
        _style = style;
        RebuildFormattedText();
        InvokeRendererDirty();
    }

    public void SetBounds(UnitBounds bounds)
    {
        _bounds = bounds;
        RebuildFormattedText();
        InvokeRendererDirty();
    }
    
    public void SetText(string text)
    {
        _text = text;
        RebuildFormattedText();
        InvokeRendererDirty();
    }
    
    public void Render(DrawingContext dc)
    {
        if (_formattedText is null || string.IsNullOrEmpty(_text))
        {
            return;
        }

        var clipRect = _bounds.Millimeters;

        dc.PushTransform(_transform);
        dc.PushClip(new RectangleGeometry(clipRect));
        dc.DrawText(_formattedText, clipRect.TopLeft);
        dc.Pop();
        dc.Pop();
    }
    
    private void RebuildFormattedText()
    {
        if (string.IsNullOrEmpty(_text))
        {
            _formattedText = null;
            return;
        }

        var fontFamily = ResolveFont(_style.Font);

        _formattedText = new FormattedText(
            _text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            Unit.FromFontSizePoints(_style.Size).Millimeters,
            new SolidColorBrush(_style.Color),
            1.0)
        {
            Trimming = TextTrimming.None
        };

        var size = _bounds.Size;
        
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

    private void InvokeRendererDirty()
    {
        RendererDirty?.Invoke();
    }
}
