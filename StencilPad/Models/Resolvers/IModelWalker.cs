using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public interface IModelWalker : IDisposable
{
    void SetTransform(UnitTransform transform);

    IModelWalker CreateModelWalker(IModelResolver resolver);
    IStyledGeometryWalker CreateStyledGeometryWalker();
    ITextWalker CreateTextWalker();
    IImageWalker CreateImageWalker();
}
