namespace StencilPad.Models;

public record LineStyleResourceId : ResourceId
{
    public static readonly LineStyleResourceId Solid = new(0);
    public static readonly LineStyleResourceId Dashes = new(1);

    public static LineStyleResourceId FromValue(int id)
    {
        return new LineStyleResourceId(id);
    }
    
    private LineStyleResourceId(int id) : base(id)
    { }
}
