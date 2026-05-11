using StencilPad.Spatial;

namespace StencilPad.Models;

public class Ruler : SheetElement<Ruler>
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

    public Unit Length => (End - Start).Magnitude;

    public Ruler()
    {
        HandleSet = new StartEndHandleSet(Unit2D.Zero, Unit2D.Zero);
    }
    
    public Ruler(Unit2D start, Unit2D end)
    {
        HandleSet = new StartEndHandleSet(start, end);
    }
    
    public override void MirrorX(Unit centerY)
    {
    }
    
    public override void MirrorY(Unit centerX)
    {
    }

    public override void Translate(Unit2D delta)
    {
        HandleSet.Start += delta;
        HandleSet.End += delta;
    }
    
    public override void AssignFrom(Ruler other)
    {
        Start = other.Start;
        End = other.End;
    }

    public override Ruler DeepClone()
    {
        var clone = new Ruler();
        
        clone.Id = Id;
        clone.AssignFrom(this);
        
        return clone;
    }
}
