using StencilPad.Spatial;

namespace StencilPad.Models;

public interface IEditablePolygonSet : IEnumerable<EditablePolygon>
{
    IHandleSource HandleSource { get; }
    
    EditablePolygon this[int index] { get; }
    int Count { get; }

    void Clear();

    event Action<EditablePolygon>? PolygonAdded;
    event Action<EditablePolygon>? PolygonRemoved;
}
