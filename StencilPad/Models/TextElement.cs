using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class TextElement : SheetElement<TextElement>
{
    public override MinMaxHandleSource HandleSource { get; }

    public Unit2D Min
    {
        get => HandleSource.Min;
        set => HandleSource.Min = value;
    }

    public Unit2D Max
    {
        get => HandleSource.Max;
        set => HandleSource.Max = value;
    }

    public Unit2D Size => Max - Min;

    private string _text = "";
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                OnPropertyChanged();
            }
        }
    }

    private string _fontName = "Arial";
    public string FontName
    {
        get => _fontName;
        set
        {
            if (_fontName != value)
            {
                _fontName = value;
                OnPropertyChanged();
            }
        }
    }

    private double _fontSize = 5.0;
    public double FontSize
    {
        get => _fontSize;
        set
        {
            if (_fontSize != value)
            {
                _fontSize = value;
                OnPropertyChanged();
            }
        }
    }

    private Color _color = Color.FromArgb(255, 0, 0, 0);
    public Color Color
    {
        get => _color;
        set
        {
            if (_color != value)
            {
                _color = value;
                OnPropertyChanged();
            }
        }
    }

    public event Action? GeometryChanged;
    
    public TextElement()
    {
        HandleSource = new MinMaxHandleSource(Unit2D.Zero, Unit2D.Zero);
        HandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
    }

    public TextElement(Unit2D start, string text)
    {
        HandleSource = new MinMaxHandleSource(start, start);
        HandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
        _text = text;
    }

    public override void MirrorX(Unit centerY)
    {
        Min = new Unit2D(Min.X, (centerY * 2) - Min.Y);
        Max = new Unit2D(Max.X, (centerY * 2) - Max.Y);
    }

    public override void MirrorY(Unit centerX)
    {
        Min = new Unit2D((centerX * 2) - Min.X, Min.Y);
        Max = new Unit2D((centerX * 2) - Max.X, Max.Y);
    }

    public override void Translate(Unit2D delta)
    {
        HandleSource.Min += delta;
        HandleSource.Max += delta;
    }

    public override void AssignFrom(TextElement other)
    {
        Min = other.Min;
        Max = other.Max;
        Text = other.Text;
        FontName = other.FontName;
        FontSize = other.FontSize;
        Color = other.Color;
    }

    public override TextElement DeepClone()
    {
        var clone = new TextElement();
        clone.Id = Id;
        clone.AssignFrom(this);
        return clone;
    }
}
