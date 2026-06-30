namespace StencilPad.Models.Resolvers;

public interface ISheetElementResolver : ISheetElementOutliner, IDisposable
{
    void Attach(IModelWalker walker);
    void Detach();
}
