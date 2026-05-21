using StencilPad.Spatial;

namespace StencilPad.Models;

public class Ruler : SheetElement<Ruler>
{
    private MinMaxHandleSource _minMaxHandleSource;

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

    public event Action? GeometryChanged;
    
    public Ruler()
    {
        _minMaxHandleSource = new MinMaxHandleSource(Unit2D.Zero, Unit2D.Zero);
        _minMaxHandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
        SetHandleSource(_minMaxHandleSource);
    }
    
    public Ruler(Unit2D start, Unit2D end)
    {
        _minMaxHandleSource = new MinMaxHandleSource(start, end);
        _minMaxHandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
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

    public override void Translate(Unit2D delta)
    {
        Transform = Transform with { Position = Transform.Position + delta };
    }

    public override void NormalizePosition()
    {
        var midpoint = (_minMaxHandleSource.Min + _minMaxHandleSource.Max) / 2;
        _minMaxHandleSource.Min -= midpoint;
        _minMaxHandleSource.Max -= midpoint;
        Transform = Transform with { Position = Transform.Position + Transform.Rotate(midpoint) };
    }

    public override UnitBounds GetBounds() => UnitBounds.FromMinMax(_minMaxHandleSource.Min, _minMaxHandleSource.Max);

    public override void AssignFrom(Ruler other)
    {
        _minMaxHandleSource.AssignFrom(other._minMaxHandleSource);
        Transform = other.Transform;
    }

    public override Ruler DeepClone()
    {
        var clone = new Ruler();
        
        clone.Id = Id;
        clone.AssignFrom(this);
        
        return clone;
    }
}
