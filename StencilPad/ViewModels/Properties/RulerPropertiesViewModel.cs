using System.Windows.Media;
using StencilPad.Models;

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

            foreach (var element in Elements)
            {
                element.FontSize = value;
            }

            OnPropertyChanged();
        }
    }

    public RulerPropertiesViewModel(Sheet sheet)
        : base(sheet)
    {
        OnElementsChanged();
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
