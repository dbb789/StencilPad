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
    private Unit2D? _textFieldPosition;
    private Unit2D? _textFieldSize;

    public event Action<UnitBounds, string>? OnTextPlaced;
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
        CancelEdit();
        
        base.Dispose();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (_textField is not null)
        {
            CommitEdit();
            return;
        }

        var mousePosition = _viewport.FromPoint(e.GetPosition(this));
        var (textElement, parentTransform) = GetTextElementAtPosition(Sheet.Elements, mousePosition, UnitTransform.Identity);
        
        if (textElement is not null)
        {
            var transform = parentTransform * textElement.Transform;
            
            ShowTextField(transform.Apply(textElement.Bounds.NW),
                          textElement.Bounds.Size,
                          (double)transform.Angle,
                          textElement);
            return;
        }
        
        var position = _unitSnap.UnitSnap(mousePosition, _unitSnapContext) ?? mousePosition;

        ShowTextField(position);

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
                if (textElement.Bounds.Contains((parentTransform * textElement.Transform).InverseApply(position)))
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

    private void ShowTextField(Unit2D position)
    {
        _textFieldPosition = position;
        _textFieldSize = null;
        _editingTextElement = null;
        
        _textField = new InlineTextField
        {
            Text = "",
            TextFontFamily = new FontFamily(DefaultFontFamilyName),
            TextFontSize = _viewport.ToPixels(Unit.FromFontSizePoints(DefaultFontSize)),
            Rotation = 0
        };

        _textField.Committed += CommitEdit;
        _textField.Cancelled += CancelEdit;

        Children.Add(_textField);

        UpdateTextFieldPosition();
    }
    
    private void ShowTextField(Unit2D position,
                               Unit2D size,
                               double rotation,
                               TextElement textElement)
    {
        _textFieldPosition = position;
        _textFieldSize = size;
        _editingTextElement = textElement;
        
        _textField = new InlineTextField
        {
            Text = textElement.Text,
            TextFontFamily = new FontFamily(DefaultFontFamilyName),
            TextFontSize = _viewport.ToPixels(Unit.FromFontSizePoints(DefaultFontSize)),
            Rotation = -rotation
        };

        _textField.Committed += CommitEdit;
        _textField.Cancelled += CancelEdit;

        Children.Add(_textField);
        
        UpdateTextFieldPosition();
    }

    public void CommitEdit()
    {
        if (_textField is null)
        {
            return;
        }

        var text = _textField.Text;
        var size = _textField.TextSize;

        if (_editingTextElement is not null)
        {
            OnTextUpdated?.Invoke(_editingTextElement, text);
        }
        else if (_textFieldPosition.HasValue && !string.IsNullOrWhiteSpace(text))
        {
            // NOTE: Click is at the top left of the text.
            var bounds = UnitBounds.FromMinMax(_textFieldPosition.Value - new Unit2D(Unit.Zero, size.Y),
                                               _textFieldPosition.Value + new Unit2D(size.X, Unit.Zero));
            
            OnTextPlaced?.Invoke(bounds, text);
            _textFieldPosition = null;
        }

        CancelEdit();
    }

    private void CancelEdit()
    {
        if (_textField is null)
        {
            return;
        }
        
        Children.Remove(_textField);
        _textField = null;
    }

    private void UpdateTextFieldPosition()
    {
        if (_textField is not null)
        {
            _textField.TextFontSize = _viewport.ToPixels(Unit.FromFontSizePoints(DefaultFontSize));

            if (_textFieldPosition.HasValue)
            {
                var screenPosition = _viewport.ToPoint(_textFieldPosition.Value);

                SetLeft(_textField, screenPosition.X);
                SetTop(_textField, screenPosition.Y);

            }

            if (_textFieldSize.HasValue)
            {
                _textField.Width = _viewport.ToPixels(_textFieldSize.Value.X) + 12;
                _textField.Height = _viewport.ToPixels(_textFieldSize.Value.Y) + 6;
            }
        }
    }

    private void OnViewportChanged()
    {
        UpdateTextFieldPosition();
        InvalidateVisual();
    }
}
