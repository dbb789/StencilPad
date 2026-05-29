namespace StencilPad.Models.Operations;

public interface IEditContext : IDisposable
{
    void Discard();
}
