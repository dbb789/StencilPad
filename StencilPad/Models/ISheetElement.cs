using StencilPad.Spatial;

namespace StencilPad.Models;

public interface ISheetElement
{
    Guid Id { get; }

    UnitTransform Transform { get; set; }
    UnitTransform ParentTransform { get; set; }
    UnitTransform WorldTransform { get; }
    
    event Action<ISheetElement>? WorldTransformChanged;
    event Action<ISheetElement>? GeometryChanged;
    
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
    void NormalizePosition();
    UnitBounds GetBounds(UnitTransform transform);
    UnitBounds GetTransformedBounds();
    bool ContainsPoint(Unit2D point);
    bool IntersectsBounds(UnitBounds bounds);
    void SetBounds(UnitBounds newBounds, UnitTransform transform);
    void AssignFromElement(ISheetElement other);
    ISheetElement DeepClone();
}
