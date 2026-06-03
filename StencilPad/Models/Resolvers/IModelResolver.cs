namespace StencilPad.Models.Resolvers;

public interface IModelResolver : IDisposable
{
    void Attach(IModelWalker walker);
    void Detach();
}
