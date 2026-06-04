using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public interface IImageWalker : IDisposable
{
    void SetBounds(UnitBounds? bounds);
    void SetImageData(byte [] imageData);
}
