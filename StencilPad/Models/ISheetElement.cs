using StencilPad.Spatial;

namespace StencilPad.Models;

public interface ISheetElement
{
    Guid Id { get; }
    IHandleSource HandleSource { get; }

    void MirrorX(Unit centerY);
    void MirrorY(Unit centerX);
    void Translate(Unit2D delta);
    void AssignFromElement(ISheetElement other);
    ISheetElement DeepClone();
}
