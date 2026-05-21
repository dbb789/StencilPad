using StencilPad.Spatial;

namespace StencilPad.Models;

public interface ISheetElement
{
    Guid Id { get; }

    UnitTransform Transform { get; set; }
    event Action<ISheetElement>? TransformChanged;
    
    event Action<ISheetElement, Handle, Unit2D, bool>? HandleAdded;
    event Action<ISheetElement, Handle>? HandleRemoved;
    event Action<ISheetElement, Handle, Unit2D>? HandleMoved;
    event Action<ISheetElement, Handle, bool>? HandleSelectionChanged;

    void QueryHandles(Action<Handle, Unit2D, bool> func);
    void SetHandleSelected(Handle handle, bool selected);
    Unit2D GetPoint(Handle handle);
    void SetPoint(Handle handle, Unit2D position);

    void MirrorX(Unit centerY);
    void MirrorY(Unit centerX);
    void Translate(Unit2D delta);
    void NormalizePosition();
    UnitBounds GetBounds(UnitTransform transform);
    UnitBounds GetTransformedBounds();
    void AssignFromElement(ISheetElement other);
    ISheetElement DeepClone();
}
