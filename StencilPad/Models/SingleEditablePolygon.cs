using System.Collections;

using StencilPad.Spatial;

namespace StencilPad.Models;

public class SingleEditablePolygon : IEditablePolygonSet
{
    public EditablePolygon Polygon { get; }
    public IHandleSet HandleSet => Polygon;

    public event Action<EditablePolygon>? PolygonAdded { add { } remove { } }
    public event Action<EditablePolygon>? PolygonRemoved { add { } remove { } }
    
    public SingleEditablePolygon()
    {
        Polygon = new EditablePolygon();
    }

    public SingleEditablePolygon(Polygon polygon)
    {
        var editablePolygon = new EditablePolygon();
        
        editablePolygon.AssignFrom(polygon);
        
        Polygon = editablePolygon;
    }

    public void AssignFrom(SingleEditablePolygon other)
    {
        Polygon.AssignFrom(other.Polygon);
    }
    
    public IEnumerator<EditablePolygon> GetEnumerator()
    {
        yield return Polygon;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        yield return Polygon;
    }
}
