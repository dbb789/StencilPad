namespace StencilPad.Models.Resolvers;

public interface IStyledGeometryResolver : IDisposable
{
    void Subscribe(IStyledGeometryWalker walker);
    void Unsubscribe(IStyledGeometryWalker walker);
    void VisitAll(IStyledGeometryWalker walker);
}
