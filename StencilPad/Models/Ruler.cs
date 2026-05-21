using StencilPad.Spatial;

namespace StencilPad.Models;

public class Ruler : SheetElement<Ruler>
{
    public override MinMaxHandleSource HandleSource { get; }

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
        get => HandleSource.Min;
        set => HandleSource.Min = value;
    }
    
    public Unit2D Max
    {
        get => HandleSource.Max;
        set => HandleSource.Max = value;
    }

    public Unit Length => (Max - Min).Magnitude;

    public event Action? GeometryChanged;
    
    public Ruler()
    {
        HandleSource = new MinMaxHandleSource(Unit2D.Zero, Unit2D.Zero);
        HandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
    }
    
    public Ruler(Unit2D start, Unit2D end)
    {
        HandleSource = new MinMaxHandleSource(start, end);
        HandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
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
    
    public override void AssignFrom(Ruler other)
    {
        HandleSource.AssignFrom(other.HandleSource);
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
