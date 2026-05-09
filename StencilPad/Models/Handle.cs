namespace StencilPad.Models;

public readonly record struct Handle(HandleType Type, int Index)
{
    public bool CanGroupMove => Type == HandleType.Vertex || Type == HandleType.Bounds;
    
    public static Handle Vertex(int index) => new(HandleType.Vertex, index);
    public static Handle Bounds(int index) => new(HandleType.Bounds, index);
    public static Handle ControlBegin(int index) => new(HandleType.ControlBegin, index);
    public static Handle ControlEnd(int index) => new(HandleType.ControlEnd, index);
}
