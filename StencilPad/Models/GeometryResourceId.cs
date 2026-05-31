namespace StencilPad.Models;

public record GeometryResourceId : ResourceId
{
    public static readonly GeometryResourceId None = new(0);
    public static readonly GeometryResourceId First = new(1);
    public static readonly GeometryResourceId DefaultMarker = new(1001);

    public static GeometryResourceId FromValue(int id)
    {
        return new GeometryResourceId(id);
    }
    
    private GeometryResourceId(int id) : base(id)
    { }
}
