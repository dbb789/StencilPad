using System.Collections;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class EditablePolygonList : IEditablePolygonSet
{
    public IHandleSet HandleSet => _handleSet;

    public Unit2D Position
    {
        get => _handleSet.Position;
        set => _handleSet.Position = value;
    }
    
    private List<EditablePolygon> _polygons;
    private GroupHandleSet _handleSet;
    
    public EditablePolygon this[int index] => _polygons[index];
    public int Count => _polygons.Count;

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

        int polygonCount = other._polygons.Count;
        
        _polygons = new(polygonCount);

        for (int i = 0; i < polygonCount; ++i)
        {
            var polygon = other._polygons[i].DeepClone();
            
            _polygons.Add(polygon);
            PolygonAdded?.Invoke(polygon);
        }

        _handleSet = new GroupHandleSet(_polygons);
        _handleSet.Position = other._handleSet.Position;
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
