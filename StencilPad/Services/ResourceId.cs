namespace StencilPad.Services;

public record ResourceId
{
    private string Id { get; init; }
    
    protected ResourceId(string id)
    {
        Id = id;
    }
}
