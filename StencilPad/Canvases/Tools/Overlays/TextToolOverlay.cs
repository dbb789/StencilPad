using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using StencilPad.Canvases.Common;
using StencilPad.Models;
using StencilPad.Spatial;

using StencilPad.Canvases.Tools.Widgets;

namespace StencilPad.Canvases.Tools.Overlays;

public class TextToolOverlay : ToolOverlay, IDisposable
{
    public const string DefaultFontFamilyName = "Arial";
    public const double DefaultFontSize = 12.0;

    private readonly IViewport _viewport;
    private readonly IUnitSnap _unitSnap;
    private readonly IUnitSnapContext _unitSnapContext;
    private InlineTextField? _textField;
    private TextElement? _editingTextElement;
    private Unit2D? _pendingPosition;

    public event Action<Unit2D, Unit2D, string>? OnTextPlaced;
    public event Action<TextElement, string>? OnTextUpdated;

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

        var mousePosition = _viewport.FromPoint(e.GetPosition(this));
        var (textElement, parentTransform) = GetTextElementAtPosition(Sheet.Elements, mousePosition, UnitTransform.Identity);
        
        if (textElement is not null)
        {
            var transform = parentTransform * textElement.Transform;

            ShowTextField(transform.Apply(textElement.Min),
                          (double)transform.Angle,
                          textElement.Text);
            _editingTextElement = textElement;
            return;
        }
        
        var snapPosition = _unitSnap.UnitSnap(mousePosition, _unitSnapContext);

        if (snapPosition.HasValue)
        {
            _pendingPosition = snapPosition.Value;
        }

        ShowTextField(mousePosition);
        e.Handled = true;
    }

    // We should break this out into a utility method if we start using it anywhere else.
    private (TextElement?, UnitTransform) GetTextElementAtPosition(IEnumerable<ISheetElement> elements,
                                                                   Unit2D position,
                                                                   UnitTransform parentTransform)
    {
        foreach (var element in elements.Reverse())
        {
            if (element is TextElement textElement)
            {
                var elementBounds = textElement.GetBounds(parentTransform * textElement.Transform);

                if (elementBounds.Contains(position))
                {
                    return (textElement, parentTransform);
                }
            }
            else if (element is ElementGroup group)
            {
                var (childElement, childElementTransform) = GetTextElementAtPosition(group.Children,
                                                                                     position,
                                                                                     parentTransform * group.Transform);
                
                if (childElement is not null)
                {
                    return (childElement, childElementTransform);
                }
            }
        }

        return (null, parentTransform);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        RenderOverlay(dc);
    }

    private void ShowTextField(Unit2D point, double rotation = 0.0, string text = "")
    {
        var screenPosition = _viewport.ToPoint(point);
        
        _textField = new InlineTextField
        {
            Text = text,
            TextFontFamily = new FontFamily(DefaultFontFamilyName),
            TextFontSize = _viewport.ToPixels(Unit.FromFontSizePoints(DefaultFontSize)),
            Rotation = rotation
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
        var size = _textField.TextSize;
        
        Children.Remove(_textField);
        _textField = null;

        if (commit)
        {
            if (_editingTextElement is not null)
            {
                OnTextUpdated?.Invoke(_editingTextElement, text);
            }
            else if (_pendingPosition.HasValue && !string.IsNullOrWhiteSpace(text))
            {
                OnTextPlaced?.Invoke(_pendingPosition.Value, size, text);
                _pendingPosition = null;
            }
        }
    }

    private void OnViewportChanged()
    {
        if (_textField is not null && _pendingPosition.HasValue)
        {
            _textField.TextFontSize = _viewport.ToPixels(Unit.FromFontSizePoints(DefaultFontSize));

            var newScreenPos = _viewport.ToPoint(_pendingPosition.Value);
            
            SetLeft(_textField, newScreenPos.X);
            SetTop(_textField, newScreenPos.Y);
        }

        InvalidateVisual();
    }
}
