using StencilPad.Spatial;

namespace StencilPad.Models;

public class Ruler : SheetElement<Ruler>
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
    
    public override void AssignFrom(Ruler other)
    {
        HandleSource.AssignFrom(other.HandleSource);
    }

    public override Ruler DeepClone()
    {
        var clone = new Ruler();
        
        clone.Id = Id;
        clone.AssignFrom(this);
        
        return clone;
    }
}
