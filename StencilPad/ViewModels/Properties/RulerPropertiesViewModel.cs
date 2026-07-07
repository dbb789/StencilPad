using System.Windows.Media;
using StencilPad.Common;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.ViewModels.Properties;

public class RulerPropertiesViewModel : ElementPropertiesViewModel<Ruler>
{
    public string Title => "Ruler Properties";

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
    
    private Unit? _minX;
    public Unit? MinX
    {
        get => _minX;
        set
        {
            _minX = value;

            if (_minX is not null)
            {
                SetElementProperty(e => e.Min = e.Min with { X = _minX.Value });
            }
            
            OnPropertyChanged();
        }
    }

    private Unit? _minY;
    public Unit? MinY
    {
        get => _minY;
        set
        {
            _minY = value;

            if (_minY is not null)
            {
                SetElementProperty(e => e.Min = e.Min with { Y = _minY.Value });
            }
            
            OnPropertyChanged();
        }
    }
    
    private Unit? _maxX;
    public Unit? MaxX
    {
        get => _maxX;
        set
        {
            _maxX = value;

            if (_maxX is not null)
            {
                SetElementProperty(e => e.Max = e.Max with { X = _maxX.Value });
            }

            OnPropertyChanged();
        }
    }

    private Unit? _maxY;
    public Unit? MaxY
    {
        get => _maxY;
        set
        {
            _maxY = value;

            if (_maxY is not null)
            {
                SetElementProperty(e => e.Max = e.Max with { Y = _maxY.Value });
            }
            
            OnPropertyChanged();
        }
    }

    private Unit? _length;
    public Unit? Length
    {
        get => _length;
        set
        {
            _length = value;

            if (_length is not null)
            {
                foreach (var element in Elements)
                {
                    var offset = element.Max - element.Min;

                    element.Max = element.Min + offset.NormalizedTo(_length.Value);
                }   
            }
            
            OnPropertyChanged();
        }
    }

    private IDisposable? _dragContext;
    
    public RulerPropertiesViewModel(Sheet sheet,
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
        _minX = All(e => e.Min.X);
        OnPropertyChanged(nameof(MinX));

        _minY = All(e => e.Min.Y);
        OnPropertyChanged(nameof(MinY));

        _maxX = All(e => e.Max.X);
        OnPropertyChanged(nameof(MaxX));

        _maxY = All(e => e.Max.Y);
        OnPropertyChanged(nameof(MaxY));
        
        _length = All(e => e.Length);
        OnPropertyChanged(nameof(Length));

        _color = Mode(e => e.Color);
        OnPropertyChanged(nameof(Color));

        _fontName = Mode(e => e.FontName) ?? "";
        OnPropertyChanged(nameof(FontName));

        _fontSize = Mode(e => e.FontSize);
        OnPropertyChanged(nameof(FontSize));
    }
}
