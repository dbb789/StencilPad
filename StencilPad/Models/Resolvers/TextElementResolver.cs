using System.ComponentModel;
using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public class TextElementResolver : IModelResolver
{
    private readonly TextElement _textElement;

    private IModelWalker? _walker;
    private ITextWalker? _textWalker;
    
    private TextStyle _textStyle;
    
    public TextElementResolver(TextElement textElement)
    {
        _textElement = textElement;
        _textStyle = CreateTextStyle();
            
        _textElement.GeometryChanged += OnGeometryChanged;
        _textElement.TransformChanged += OnTransformChanged;
        _textElement.PropertyChanged += OnPropertyChanged;
    }

    public void Dispose()
    {
        Detach();
        
        _textElement.GeometryChanged -= OnGeometryChanged;
        _textElement.TransformChanged -= OnTransformChanged;
        _textElement.PropertyChanged -= OnPropertyChanged;
    }

    public void Attach(IModelWalker walker)
    {
        _walker = walker;
        _walker.SetTransform(_textElement.Transform);
        
        _textWalker = walker.CreateTextWalker();
        _textWalker.SetStyle(_textStyle);
        _textWalker.SetBounds(UnitBounds.FromMinMax(_textElement.Min, _textElement.Max));
        _textWalker.SetText(_textElement.Text);
    }

    public void Detach()
    {
        _textWalker = null;
        _walker = null;
    }
    
    private void OnGeometryChanged(ISheetElement element)
    {
        _textWalker?.SetBounds(UnitBounds.FromMinMax(_textElement.Min, _textElement.Max));
    }

    private void OnTransformChanged(ISheetElement element)
    {
        _walker?.SetTransform(_textElement.Transform);
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
    }

    private TextStyle CreateTextStyle()
    {
        return new TextStyle
        {
            Font = _textElement.FontName,
            Size = _textElement.FontSize,
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
