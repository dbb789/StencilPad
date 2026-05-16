namespace StencilPad.Canvases.Common;

public class EmptyUnitSnapContext : BaseUnitSnapContext
{
    public static readonly IUnitSnapContext Instance = new EmptyUnitSnapContext();
}
