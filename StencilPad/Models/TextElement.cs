using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class TextElement : SheetElement<TextElement>
{
    private BoundsHandleSource _boundsHandleSource;

    public Unit2D Min
    {
        get => _boundsHandleSource.Bounds.Min;
        set => _boundsHandleSource.Bounds = UnitBounds.FromMinMax(value, _boundsHandleSource.Bounds.Max);
    }

    public Unit2D Max
    {
        get => _boundsHandleSource.Bounds.Max;
        set => _boundsHandleSource.Bounds = UnitBounds.FromMinMax(_boundsHandleSource.Bounds.Min, value);
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
        _boundsHandleSource = new BoundsHandleSource(UnitBounds.Empty);
        _boundsHandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
        SetHandleSource(_boundsHandleSource);
    }

    public TextElement(Unit2D start, string text)
    {
        _boundsHandleSource = new BoundsHandleSource(UnitBounds.FromMinMax(start, start));
        _boundsHandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
        SetHandleSource(_boundsHandleSource);
        _text = text;
    }

    public override void MirrorX(Unit centerY)
    {
        Transform = Transform with 
        { 
            Position = Transform.Position with { Y = (centerY * 2) - Transform.Position.Y },
            Angle = -Transform.Angle
        };
    }

    public override void MirrorY(Unit centerX)
    {
        Transform = Transform with 
        { 
            Position = Transform.Position with { X = (centerX * 2) - Transform.Position.X },
            Angle = -Transform.Angle
        };
    }

    public override void Translate(Unit2D delta)
    {
        Transform = Transform with { Position = Transform.Position + delta };
    }

    public override void AssignFrom(TextElement other)
    {
        _boundsHandleSource.AssignFrom(other._boundsHandleSource);
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
