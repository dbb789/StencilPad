using System.Windows.Media;
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
            
            using var context = _operationService.TryCreateEditContext(_sheet, Elements);

            foreach (var element in Elements)
            {
                element.Color = value;
            }

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
            
            using var context = _operationService.CreateEditContext(_sheet, Elements);

            foreach (var element in Elements)
            {
                element.FontName = value;
            }

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
            
            using var context = _operationService.CreateEditContext(_sheet, Elements);

            foreach (var element in Elements)
            {
                element.FontSize = value;
            }

            OnPropertyChanged();
        }
    }
    
    private readonly Sheet _sheet;
    private readonly IOperationService _operationService;
    private IDisposable? _dragContext;

    public TextPropertiesViewModel(Sheet sheet,
                                   IOperationService operationService)
        : base(sheet)
    {
        _sheet = sheet;
        _operationService = operationService;

        OnElementsChanged();
    }
    
    public void DragBegin()
    {
        _dragContext = _operationService.CreateEditContext(_sheet, Elements);
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
