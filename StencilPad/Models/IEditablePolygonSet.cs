namespace StencilPad.Models;

public interface IEditablePolygonSet : IEnumerable<EditablePolygon>
{
    IHandleSet HandleSet { get; }
    
    EditablePolygon this[int index] { get; }
    int Count { get; }

    event Action<EditablePolygon>? PolygonAdded;
    event Action<EditablePolygon>? PolygonRemoved;
}
