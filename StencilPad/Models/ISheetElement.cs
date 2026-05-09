using StencilPad.Spatial;

namespace StencilPad.Models;

public interface ISheetElement
{
    Guid Id { get; }
    
    void Translate(Unit2D delta);
    void AssignFromElement(ISheetElement other);
    ISheetElement DeepClone();
}
