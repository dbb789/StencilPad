using StencilPad.Spatial;

namespace StencilPad.Models;

public interface IEditablePolygonSet : IEnumerable<EditablePolygon>
{
    IHandleSet HandleSet { get; }

    event Action<EditablePolygon>? PolygonAdded;
    event Action<EditablePolygon>? PolygonRemoved;
}
