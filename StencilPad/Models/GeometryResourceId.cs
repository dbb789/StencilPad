namespace StencilPad.Models;

public record GeometryResourceId : ResourceId
{
    public static readonly GeometryResourceId None = new(0);
    public static readonly GeometryResourceId Arrow0 = new(1);

    private GeometryResourceId(int id) : base(id)
    { }
}
