namespace StencilPad.Rendering;

public record ResourceId
{
    private int Id { get; init; }
    
    protected ResourceId(int id)
    {
        Id = id;
    }
}
