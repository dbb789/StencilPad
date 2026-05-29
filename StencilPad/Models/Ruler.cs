using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class Ruler : SheetElement<Ruler>
{
    private MinMaxHandleSource _minMaxHandleSource;

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

    private bool _showGuides = true;
    public bool ShowGuides
    {
        get => _showGuides;
        set
        {
            if (_showGuides != value)
            {
                _showGuides = value;
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
        var midpoint = (_minMaxHandleSource.Min + _minMaxHandleSource.Max) / 2;
        _minMaxHandleSource.Min -= midpoint;
        _minMaxHandleSource.Max -= midpoint;
        Transform = Transform with { Position = Transform.Position + Transform.Rotate(midpoint) };
    }

    public override UnitBounds GetBounds(UnitTransform transform)
    {
        return UnitBounds.FromMinMax(_minMaxHandleSource.Min, _minMaxHandleSource.Max).ApplyTransform(transform);
    }

    public override void SetBounds(UnitBounds newBounds, UnitTransform transform)
    {
        _minMaxHandleSource.Min = transform.InverseApply(newBounds.Min);
        _minMaxHandleSource.Max = transform.InverseApply(newBounds.Max);
    }

    public override void AssignFrom(Ruler other)
    {
        _minMaxHandleSource.AssignFrom(other._minMaxHandleSource);
        Transform = other.Transform;
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
