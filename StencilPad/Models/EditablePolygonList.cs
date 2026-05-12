using System.Collections;

namespace StencilPad.Models;

public class EditablePolygonList : IEditablePolygonSet
{
    public IHandleSet HandleSet => _handleSet;
    
    private List<EditablePolygon> _polygons;
    private GroupHandleSet _handleSet;

    public event Action<EditablePolygon>? PolygonAdded;
    public event Action<EditablePolygon>? PolygonRemoved;
    
    public EditablePolygonList()
    {
        _polygons = [];
        _handleSet = new GroupHandleSet();
    }

    public void Add(EditablePolygon polygon)
    {
        _polygons.Add(polygon);
        _handleSet.Add(polygon);

        PolygonAdded?.Invoke(polygon);
    }

    public void AssignFrom(EditablePolygonList other)
    {
        foreach (var polygon in _polygons)
        {
            PolygonRemoved?.Invoke(polygon);
        }
        
        _polygons = new(other._polygons.Select(p => p.DeepClone()));
        _handleSet = new GroupHandleSet(_polygons);
        _handleSet.SetSelectedHandles(other.HandleSet.GetSelectedHandles());
    }

    public List<EditablePolygon>.Enumerator GetEnumerator()
    {
        return _polygons.GetEnumerator();
    }
    
    IEnumerator<EditablePolygon> IEnumerable<EditablePolygon>.GetEnumerator()
    {
        return _polygons.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _polygons.GetEnumerator();
    }
}
