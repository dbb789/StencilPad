using StencilPad.Spatial;

namespace StencilPad.Models;

public class Ruler : SheetElement<Ruler>
{
    public override StartEndHandleSource HandleSource { get; }

    public Unit2D Start
    {
        get => HandleSource.Start;
        set => HandleSource.Start = value;
    }
    
    public Unit2D End
    {
        get => HandleSource.End;
        set => HandleSource.End = value;
    }

    public Unit Length => (End - Start).Magnitude;

    public event Action? GeometryChanged;
    
    public Ruler()
    {
        HandleSource = new StartEndHandleSource(Unit2D.Zero, Unit2D.Zero);
        HandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
    }
    
    public Ruler(Unit2D start, Unit2D end)
    {
        HandleSource = new StartEndHandleSource(start, end);
        HandleSource.HandleMoved += (_, _, _) => GeometryChanged?.Invoke();
    }
    
    public override void MirrorX(Unit centerY)
    {
        Start = new Unit2D(Start.X, (centerY * 2) - Start.Y);
        End = new Unit2D(End.X, (centerY * 2) - End.Y);
    }
    
    public override void MirrorY(Unit centerX)
    {
        Start = new Unit2D((centerX * 2) - Start.X, Start.Y);
        End = new Unit2D((centerX * 2) - End.X, End.Y);
    }

    public override void Translate(Unit2D delta)
    {
        HandleSource.Start += delta;
        HandleSource.End += delta;
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
