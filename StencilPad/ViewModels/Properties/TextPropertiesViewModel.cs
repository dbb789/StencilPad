using System.Windows.Media;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Services;

namespace StencilPad.ViewModels.Properties;

public class TextPropertiesViewModel : ElementPropertiesViewModel<TextElement>
{
    public string Title => "Text Properties";

    private Color _color;
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            SetElementProperty(e => e.Color = value);
            OnPropertyChanged();
        }
    }

    private string _fontName = "";
    public string FontName
    {
        get => _fontName;
        set
        {
            _fontName = value;
            SetElementProperty(e => e.FontName = value);
            OnPropertyChanged();
        }
    }

    private double _fontSize;
    public double FontSize
    {
        get => _fontSize;
        set
        {
            _fontSize = value;
            SetElementProperty(e => e.FontSize = value);
            OnPropertyChanged();
        }
    }
    
    private IDisposable? _dragContext;

    public TextPropertiesViewModel(Sheet sheet,
                                   ISettings settings,
                                   IOperationService operationService)
        : base(sheet, operationService, settings)
    {
        OnElementsChanged();
    }
    
    public void DragBegin()
    {
        _dragContext = OperationService.CreateEditContext(Sheet, Elements);
    }

    public void DragEnd()
    {
        _dragContext?.Dispose();
    }

    protected override void OnElementsChanged()
    {
        _color = Mode(e => e.Color);
        OnPropertyChanged(nameof(Color));

        _fontName = Mode(e => e.FontName) ?? "";
        OnPropertyChanged(nameof(FontName));

        _fontSize = Mode(e => e.FontSize);
        OnPropertyChanged(nameof(FontSize));
    }
}
