using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public interface IImageWalker
{
    void SetBounds(UnitBounds? bounds);
    void SetImageData(byte [] imageData);
}
