namespace StencilPad.Models;

public record ResourceId
{
    private int Id { get; init; }
    
    protected ResourceId(int id)
    {
        Id = id;
    }
    
    public int ToValue()
    {
        return Id;
    }
}
