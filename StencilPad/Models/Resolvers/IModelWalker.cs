using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public interface IModelWalker : IDisposable
{
    void SetTransform(UnitTransform transform);
    
    IStyledGeometryWalker CreateStyledGeometryWalker();
    ITextWalker CreateTextWalker();
}
