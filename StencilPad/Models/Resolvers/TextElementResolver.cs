using StencilPad.Spatial;
using System.ComponentModel;

namespace StencilPad.Models.Resolvers;

public class TextElementResolver : SheetElementResolver
{
    private readonly TextElement _textElement;

    private IModelWalker? _walker;
    private ITextWalker? _textWalker;
    
    private TextStyle _textStyle;

    public TextElementResolver(TextElement textElement)
        : base(textElement)
    {
        _textElement = textElement;
        _textStyle = CreateTextStyle();
            
        _textElement.GeometryChanged += OnGeometryChanged;
        _textElement.TransformChanged += OnTransformChanged;
        _textElement.PropertyChanged += OnPropertyChanged;
    }

    public override void Dispose()
    {
        Detach();
        
        _textElement.GeometryChanged -= OnGeometryChanged;
        _textElement.TransformChanged -= OnTransformChanged;
        _textElement.PropertyChanged -= OnPropertyChanged;
    }

    public override void Attach(IModelWalker walker)
    {
        _walker = walker;
        _walker.SetTransform(_textElement.Transform);
        
        _textWalker = walker.CreateTextWalker();
        _textWalker.SetStyle(_textStyle);
        _textWalker.SetBounds(UnitBounds.FromMinMax(_textElement.Min, _textElement.Max));
        _textWalker.SetText(_textElement.Text);
    }

    public override void Detach()
    {
        _textWalker = null;
        _walker = null;
    }
    
    private void OnGeometryChanged(ISheetElement element)
    {
        _textWalker?.SetBounds(UnitBounds.FromMinMax(_textElement.Min, _textElement.Max));

        InvokeOutlineChanged();
    }

    private void OnTransformChanged(ISheetElement element)
    {
        _walker?.SetTransform(_textElement.Transform);

        InvokeOutlineChanged();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsStyleProperty(e.PropertyName))
        {
            _textStyle = CreateTextStyle();
            _textWalker?.SetStyle(_textStyle);
        }
        else
        {
            _textWalker?.SetText(_textElement.Text);
        }

        InvokeOutlineChanged();
    }

    private TextStyle CreateTextStyle()
    {
        return new TextStyle
        {
            Font = _textElement.FontName,
            Size = _textElement.FontSize,
            Justification = _textElement.Justification,
            Color = _textElement.Color
        };
    }
    
    private static bool IsStyleProperty(string? propertyName)
    {
        return propertyName == nameof(TextElement.FontName) ||
            propertyName == nameof(TextElement.FontSize) ||
            propertyName == nameof(TextElement.Color);
    }
}
