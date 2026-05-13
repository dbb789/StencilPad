using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class TextElement : SheetElement<TextElement>
{
    public override StartEndHandleSet HandleSet { get; }

    public Unit2D Start
    {
        get => HandleSet.Start;
        set => HandleSet.Start = value;
    }

    public Unit2D End
    {
        get => HandleSet.End;
        set => HandleSet.End = value;
    }

    public Unit2D Size => End - Start;

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
        HandleSet = new StartEndHandleSet(Unit2D.Zero, Unit2D.Zero);
        HandleSet.HandleMoved += (handle, position) => GeometryChanged?.Invoke();
    }

    public TextElement(Unit2D start, string text)
    {
        HandleSet = new StartEndHandleSet(start, start);
        HandleSet.HandleMoved += (handle, position) => GeometryChanged?.Invoke();
        _text = text;
    }

    public override void MirrorX(Unit centerY)
    {
        Start = new Unit2D(Start.X, (centerY * 2) - Start.Y);
        End = new Unit2D(End.X, (centerY * 2) - End.Y);
    }

    public override void MirrorY(Unit centerX)
    {
        Start = new Unit2D((centerX * 2) - Start.X, Start.Y);
        End = new Unit2D((centerX * 2) - End.X, End.Y);
    }

    public override void Translate(Unit2D delta)
    {
        HandleSet.Start += delta;
        HandleSet.End += delta;
    }

    public override void AssignFrom(TextElement other)
    {
        Start = other.Start;
        End = other.End;
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
