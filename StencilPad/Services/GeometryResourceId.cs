namespace StencilPad.Services;

public record GeometryResourceId : ResourceId
{
    public static readonly GeometryResourceId Arrow0 = new("Arrow0");
    
    private GeometryResourceId(string id) : base(id)
    { }
}
