using System.Collections;

using StencilPad.Spatial;

namespace StencilPad.Models;

public class SingleEditablePolygon : IEditablePolygonSet
{
    public EditablePolygon Polygon { get; }
    public IHandleSource HandleSource => Polygon;

    public EditablePolygon this[int index] => Polygon;
    public int Count => 1;

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
