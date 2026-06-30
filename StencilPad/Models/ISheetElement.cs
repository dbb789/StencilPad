using StencilPad.Spatial;

namespace StencilPad.Models;

public interface ISheetElement : IHandleSource<ISheetElement>
{
    Guid Id { get; }

    UnitTransform Transform { get; set; }
    
    event Action<ISheetElement>? TransformChanged;
    event Action<ISheetElement>? GeometryChanged;

    void MirrorX(Unit centerY);
    void MirrorY(Unit centerX);
    void NormalizePosition();
    UnitBounds GetTransformedBounds(UnitTransform transform);
    UnitBounds GetBounds();
    void SetTransformedBounds(UnitBounds newBounds, UnitTransform transform);
    void AssignFromElement(ISheetElement other);
    ISheetElement DeepClone();
}
