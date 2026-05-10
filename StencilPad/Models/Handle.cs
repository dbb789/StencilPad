using StencilPad.Spatial;

namespace StencilPad.Models;

public readonly record struct Handle
{
    public static Handle Vertex(IHandleParent Parent, int index) => new(Parent, HandleType.Vertex, index);
    public static Handle Bounds(IHandleParent Parent, int index) => new(Parent, HandleType.Bounds, index);
    public static Handle ControlBegin(IHandleParent Parent, int index) => new(Parent, HandleType.ControlBegin, index);
    public static Handle ControlEnd(IHandleParent Parent, int index) => new(Parent, HandleType.ControlEnd, index);
    
    public bool CanGroupMove => Type == HandleType.Vertex || Type == HandleType.Bounds;

    private readonly IHandleParent _parent;
    public HandleType Type { get; init; }
    public int Index { get; init; }
    
    public Handle(IHandleParent parent, HandleType type, int index)
    {
        _parent = parent;
        Type = type;
        Index = index;
    }

    public Unit2D GetPoint()
    {
        return _parent.GetPoint(this);
    }

    public void SetPoint(Unit2D position)
    {
        _parent.SetPoint(this, position);
    }
}
