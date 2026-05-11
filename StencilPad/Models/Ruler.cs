using StencilPad.Spatial;

namespace StencilPad.Models;

public class Ruler : SheetElement<Ruler>
{
    public override StartEndHandleSet HandleSet { get; }

    public Unit2D Start => HandleSet.Start;
    public Unit2D End => HandleSet.End;

    public Unit Length => (End - Start).Magnitude;

    public Ruler(Unit2D start, Unit2D end)
    {
        HandleSet = new StartEndHandleSet(start, end);
    }

    public override void Translate(Unit2D delta)
    {
        HandleSet.Start += delta;
        HandleSet.End += delta;
    }
    
    public override void AssignFrom(Ruler other)
    {
        HandleSet.Start = other.HandleSet.Start;
        HandleSet.End = other.HandleSet.End;
    }

    public override Ruler DeepClone()
    {
        var clone = new Ruler(Start, End);
        
        clone.Id = Id;
        
        return clone;
    }
}
