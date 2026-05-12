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

    public event Action<Unit2D, string>? OnTextPlaced;

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
        _pendingPosition = _unitSnap.UnitSnap(_viewport.FromPoint(mousePosition));

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
            BorderThickness = new Thickness(1),
            MinWidth = 100,
            FontFamily = new FontFamily("Arial"),
            FontSize = 14,
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
            OnTextPlaced?.Invoke(_pendingPosition.Value, text);
        }

        _pendingPosition = null;
    }

    private void OnViewportChanged()
    {
        InvalidateVisual();
    }
}
