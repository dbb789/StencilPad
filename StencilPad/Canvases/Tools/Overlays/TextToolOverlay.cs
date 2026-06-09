using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Models;
using StencilPad.Spatial;

using StencilPad.Canvases.Tools.Widgets;

namespace StencilPad.Canvases.Tools.Overlays;

public class TextToolOverlay : ToolOverlay, IDisposable
{
    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;
    private readonly IUnitSnapContext _unitSnapContext;
    private InlineTextField? _textField;
    private Unit2D? _pendingPosition;

    public double FontSize { get; set; } = 12.0;
    public string FontFamilyName { get; set; } = "Arial";

    public event Action<Unit2D, Unit2D, string>? OnTextPlaced;

    public TextToolOverlay(Sheet sheet ,IViewport viewport, IUnitSnap unitSnap)
        : base(viewport, sheet, false)
    {
        _viewport = viewport;
        _unitSnap = unitSnap;
        _unitSnapContext = new DefaultUnitSnapContext(viewport);
        _viewport.ViewportChanged += OnViewportChanged;

        RegisterOverlay(TextElementToolOverlayRenderer.Factory);
    }

    public override void Dispose()
    {
        _viewport.ViewportChanged -= OnViewportChanged;
        CommitOrCancel(false);
        
        base.Dispose();
    }

    public void Commit()
    {
        CommitOrCancel(true);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (_textField is not null)
        {
            CommitOrCancel(true);
            return;
        }

        var mousePosition = e.GetPosition(this);
        var unitPosition = _viewport.FromPoint(mousePosition);
        var snapPosition = _unitSnap.UnitSnap(unitPosition, _unitSnapContext);

        if (snapPosition.HasValue)
        {
            _pendingPosition = snapPosition.Value;
        }

        ShowTextField(mousePosition);
        e.Handled = true;
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        RenderOverlay(dc);
    }

    private void ShowTextField(Point screenPosition)
    {
        _textField = new InlineTextField
        {
            TextFontFamily = new FontFamily(FontFamilyName),
            TextFontSize = _viewport.ToPixels(Unit.FromFontSizePoints(FontSize)),
        };

        _textField.Cancelled += () => CommitOrCancel(false);
        _textField.Committed += () => CommitOrCancel(true);

        Children.Add(_textField);
        SetLeft(_textField, screenPosition.X);
        SetTop(_textField, screenPosition.Y);
    }

    private void CommitOrCancel(bool commit)
    {
        if (_textField is null)
        {
            return;
        }

        var text = _textField.Text;
        Children.Remove(_textField);
        _textField = null;

        if (commit && _pendingPosition.HasValue && !string.IsNullOrWhiteSpace(text))
        {
            OnTextPlaced?.Invoke(_pendingPosition.Value, Measure(text), text);
        }

        _pendingPosition = null;
    }

    private void OnViewportChanged()
    {
        if (_textField is not null && _pendingPosition.HasValue)
        {
            _textField.TextFontSize = _viewport.ToPixels(Unit.FromFontSizePoints(FontSize));

            var newScreenPos = _viewport.ToPoint(_pendingPosition.Value);
            
            SetLeft(_textField, newScreenPos.X);
            SetTop(_textField, newScreenPos.Y);
        }

        InvalidateVisual();
    }
    
    private Unit2D Measure(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Unit2D.Zero;
        }

        var fontFamily = new FontFamily(FontFamilyName);

        var ft = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            Unit.FromFontSizePoints(FontSize).Millimeters,
            Brushes.Black,
            1.0);

        return Unit2D.FromMillimeters(ft.Width, ft.Height);
    }
}
