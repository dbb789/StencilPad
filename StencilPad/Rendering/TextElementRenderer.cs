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

    public override UnitBounds SelectionBounds
    {
        get
        {
            if (_textElement.Size == Unit2D.Zero)
            {
                return UnitBounds.Empty;
            }

            return UnitBounds.FromMinMax(_textElement.Min, _textElement.Max).ApplyTransform(_textElement.Transform);
        }
    }

    private readonly TextElement _textElement;
    private FormattedText? _formattedText;

    public TextElementRenderer(TextElement textElement)
    {
        _textElement = textElement;
        _textElement.GeometryChanged += GeometryChanged;
        _textElement.PropertyChanged += OnPropertyChanged;
        RebuildFormattedText();
    }

    public override void Dispose()
    {
        _textElement.GeometryChanged -= GeometryChanged;
        _textElement.PropertyChanged -= OnPropertyChanged;
    }

    public override bool HitTest(Unit2D unit)
    {
        var localUnit = _textElement.Transform.InverseApply(unit);
        return UnitBounds.FromMinMax(_textElement.Min, _textElement.Max).Contains(localUnit);
    }

    public override bool BoundsTest(UnitBounds bounds)
    {
        // Transform the selection bounds into the local space of the text.
        var localNW = _textElement.Transform.InverseApply(bounds.NW);
        var localNE = _textElement.Transform.InverseApply(bounds.NE);
        var localSW = _textElement.Transform.InverseApply(bounds.SW);
        var localSE = _textElement.Transform.InverseApply(bounds.SE);

        var localSelectionBounds = UnitBounds.FromMinMax(
            new Unit2D(Unit.Min(Unit.Min(localNW.X, localNE.X), Unit.Min(localSW.X, localSE.X)),
                       Unit.Min(Unit.Min(localNW.Y, localNE.Y), Unit.Min(localSW.Y, localSE.Y))),
            new Unit2D(Unit.Max(Unit.Max(localNW.X, localNE.X), Unit.Max(localSW.X, localSE.X)),
                       Unit.Max(Unit.Max(localNW.Y, localNE.Y), Unit.Max(localSW.Y, localSE.Y))));

        return localSelectionBounds.Intersects(UnitBounds.FromMinMax(_textElement.Min, _textElement.Max));
    }

    public override void Render(DrawingContext dc)
    {
        if (_formattedText is null || string.IsNullOrEmpty(_textElement.Text))
        {
            return;
        }

        var bounds = UnitBounds.FromMinMax(_textElement.Min, _textElement.Max);
        var clipRect = bounds.Millimeters;

        var transform = CreateTransform();
        dc.PushTransform(transform);
        dc.PushClip(new RectangleGeometry(clipRect));
        dc.DrawText(_formattedText, clipRect.TopLeft);
        dc.Pop();
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

    private void GeometryChanged()
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
