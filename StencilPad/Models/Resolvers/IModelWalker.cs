using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public interface IModelWalker
{
    void SetTransform(UnitTransform transform);

    IModelWalker CreateModelWalker();
    IStyledGeometryWalker CreateStyledGeometryWalker();
    ITextWalker CreateTextWalker();
    IImageWalker CreateImageWalker();
}
