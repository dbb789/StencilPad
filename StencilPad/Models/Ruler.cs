using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class Ruler : SheetElement<Ruler>
{
    private MinMaxHandleSource _minMaxHandleSource;

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

    private double _fontSize = 8.0;
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

    public Unit2D Min
    {
        get => _minMaxHandleSource.Min;
        set => _minMaxHandleSource.Min = value;
    }
    
    public Unit2D Max
    {
        get => _minMaxHandleSource.Max;
        set => _minMaxHandleSource.Max = value;
    }

    public Unit Length => (Max - Min).Magnitude;
        
    public Ruler()
    {
        _minMaxHandleSource = new MinMaxHandleSource(Unit2D.Zero, Unit2D.Zero);
        _minMaxHandleSource.HandleMoved += (_, _, _) => FireGeometryChanged();
        SetHandleSource(_minMaxHandleSource);
    }
    
    public Ruler(Unit2D start, Unit2D end)
    {
        _minMaxHandleSource = new MinMaxHandleSource(start, end);
        _minMaxHandleSource.HandleMoved += (_, _, _) => FireGeometryChanged();
        SetHandleSource(_minMaxHandleSource);
    }
    
    public override void MirrorX(Unit centerY)
    {
        var min = _minMaxHandleSource.Min;
        var max = _minMaxHandleSource.Max;

        _minMaxHandleSource.Min = new Unit2D(min.X, -min.Y);
        _minMaxHandleSource.Max = new Unit2D(max.X, -max.Y);
        
        Transform = Transform with 
        { 
            Position = Transform.Position with { Y = (centerY * 2) - Transform.Position.Y },
            Angle = -Transform.Angle
        };
    }
    
    public override void MirrorY(Unit centerX)
    {
        var min = _minMaxHandleSource.Min;
        var max = _minMaxHandleSource.Max;

        _minMaxHandleSource.Min = new Unit2D(-min.X, min.Y);
        _minMaxHandleSource.Max = new Unit2D(-max.X, max.Y);
        
        Transform = Transform with 
        { 
            Position = Transform.Position with { X = (centerX * 2) - Transform.Position.X },
            Angle = -Transform.Angle
        };
    }

    public override void NormalizePosition()
    {
        var midpoint = (_minMaxHandleSource.Min + _minMaxHandleSource.Max) / 2;
        
        _minMaxHandleSource.Min -= midpoint;
        _minMaxHandleSource.Max -= midpoint;
        Transform = Transform with { Position = Transform.Position + Transform.Rotate(midpoint) };
    }

    public override UnitBounds GetTransformedBounds(UnitTransform transform)
    {
        return UnitBounds.FromMinMax(_minMaxHandleSource.Min, _minMaxHandleSource.Max).ApplyTransform(transform);
    }

    public override void SetTransformedBounds(UnitBounds newBounds, UnitTransform transform)
    {
        _minMaxHandleSource.Min = transform.InverseApply(newBounds.Min);
        _minMaxHandleSource.Max = transform.InverseApply(newBounds.Max);
    }

    public override void AssignFrom(Ruler other)
    {
        _minMaxHandleSource.AssignFrom(other._minMaxHandleSource);
        Transform = other.Transform;
        
        FontName = other.FontName;
        FontSize = other.FontSize;
        Color = other.Color;
    }

    public override Ruler DeepClone()
    {
        var clone = new Ruler();
        
        clone.Id = Id;
        clone.AssignFrom(this);
        
        return clone;
    }
}
