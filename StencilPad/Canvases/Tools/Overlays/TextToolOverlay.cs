using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class TextToolOverlay : Canvas, IDisposable
{
    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;
    private TextBox? _textBox;
    private Unit2D? _pendingPosition;

    public double FontSize { get; set; } = 12.0;
    public string FontFamilyName { get; set; } = "Arial";

    public event Action<Unit2D, Unit2D, string>? OnTextPlaced;

    public TextToolOverlay(IViewport viewport, IUnitSnap unitSnap)
    {
        _viewport = viewport;
        _unitSnap = unitSnap;
        _viewport.ViewportChanged += OnViewportChanged;
    }

    public void Dispose()
    {
        _viewport.ViewportChanged -= OnViewportChanged;
        CommitOrCancel(commit: false);
    }

    public void Commit()
    {
        CommitOrCancel(commit: true);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (_textBox is not null)
        {
            CommitOrCancel(commit: true);
            return;
        }

        var mousePosition = e.GetPosition(this);
        var unitPosition = _viewport.FromPoint(mousePosition);
        var snapPosition = _unitSnap.UnitSnap(unitPosition, EmptyUnitSnapContext.Instance);

        if (snapPosition.HasValue)
        {
            _pendingPosition = snapPosition.Value;
        }

        ShowTextBox(mousePosition);
        e.Handled = true;
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
    }

    private void ShowTextBox(Point screenPosition)
    {
        _textBox = new TextBox
        {
            Background = Brushes.White,
            BorderBrush = Brushes.CornflowerBlue,
            MinWidth = 100,
            Padding = new Thickness(0),
            FontFamily = new FontFamily(FontFamilyName),
            FontSize = _viewport.ToPixels(Unit.FromFontSizePoints(FontSize)),
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
        };

        _textBox.KeyDown += OnTextBoxKeyDown;
        _textBox.LostFocus += OnTextBoxLostFocus;

        Children.Add(_textBox);
        SetLeft(_textBox, screenPosition.X);
        SetTop(_textBox, screenPosition.Y);

        _textBox.Focus();
    }

    private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CommitOrCancel(commit: false);
            e.Handled = true;
        }
    }

    private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
    {
        CommitOrCancel(commit: true);
    }

    private void CommitOrCancel(bool commit)
    {
        if (_textBox is null)
        {
            return;
        }

        _textBox.KeyDown -= OnTextBoxKeyDown;
        _textBox.LostFocus -= OnTextBoxLostFocus;

        var text = _textBox.Text;
        Children.Remove(_textBox);
        _textBox = null;

        if (commit && _pendingPosition.HasValue && !string.IsNullOrWhiteSpace(text))
        {
            OnTextPlaced?.Invoke(_pendingPosition.Value, Measure(text), text);
        }

        _pendingPosition = null;
    }

    private void OnViewportChanged()
    {
        if (_textBox is not null && _pendingPosition.HasValue)
        {
            _textBox.FontSize = _viewport.ToPixels(Unit.FromFontSizePoints(FontSize));

            var newScreenPos = _viewport.ToPoint(_pendingPosition.Value);
            
            SetLeft(_textBox, newScreenPos.X);
            SetTop(_textBox, newScreenPos.Y);
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
            FontSize,
            Brushes.Black,
            1.0);

        return new Unit2D(Unit.FromMillimeters(ft.Width + 0.5), Unit.FromMillimeters(ft.Height));
    }
}
