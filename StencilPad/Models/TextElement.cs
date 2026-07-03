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

    public UnitBounds Bounds => _boundsHandleSource.Bounds;
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

    private double _fontSize = 12.0;
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

    private Justification _justification = Justification.Left;
    public Justification Justification
    {
        get => _justification;
        set
        {
            if (_justification != value)
            {
                _justification = value;
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

    public TextElement()
    {
        _boundsHandleSource = new BoundsHandleSource(UnitBounds.Empty);
        _boundsHandleSource.HandleMoved += (_, _, _) => FireGeometryChanged();
        SetHandleSource(_boundsHandleSource);
    }

    public TextElement(UnitBounds bounds, string text)
    {
        _boundsHandleSource = new BoundsHandleSource(bounds);
        _boundsHandleSource.HandleMoved += (_, _, _) => FireGeometryChanged();
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

    public override void NormalizePosition()
    {
        var midpoint = _boundsHandleSource.Bounds.Center;
        
        _boundsHandleSource.Bounds = _boundsHandleSource.Bounds - midpoint;
        
        Transform = Transform with { Position = Transform.Position + Transform.Rotate(midpoint) };
    }

    public override UnitBounds GetTransformedBounds(UnitTransform transform)
    {
        return _boundsHandleSource.Bounds.ApplyTransform(transform);
    }

    public override void SetTransformedBounds(UnitBounds newBounds, UnitTransform transform)
    {
        _boundsHandleSource.Bounds = UnitBounds.FromMinMax(
            transform.InverseApply(newBounds.Min),
            transform.InverseApply(newBounds.Max));
    }

    public override void AssignFrom(TextElement other)
    {
        base.AssignFrom(other);

        _boundsHandleSource.AssignFrom(other._boundsHandleSource);
        Text = other.Text;
        AssignStyleFrom(other);
    }

    public override void AssignStyleFrom(TextElement other)
    {
        base.AssignStyleFrom(other);

        FontName = other.FontName;
        FontSize = other.FontSize;
        Justification = other.Justification;
        Color = other.Color;
    }
}
