using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class TextElement : SheetElement<TextElement>
{
    public override BoundsHandleSource HandleSource { get; }

    public override UnitTransform Transform
    {
        get => HandleSource.Transform;
        set
        {
            if (HandleSource.Transform != value)
            {
                HandleSource.Transform = value;
                OnPropertyChanged();
                GeometryChanged?.Invoke();
            }
        }
    }

    public Unit2D Min
    {
        get => HandleSource.Bounds.Min;
        set => HandleSource.Bounds = UnitBounds.FromMinMax(value, HandleSource.Bounds.Max);
    }

    public Unit2D Max
    {
        get => HandleSource.Bounds.Max;
        set => HandleSource.Bounds = UnitBounds.FromMinMax(HandleSource.Bounds.Min, value);
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
        HandleSource = new BoundsHandleSource(UnitBounds.Empty);
        HandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
    }

    public TextElement(Unit2D start, string text)
    {
        HandleSource = new BoundsHandleSource(UnitBounds.FromMinMax(start, start));
        HandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
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
        HandleSource.AssignFrom(other.HandleSource);
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
