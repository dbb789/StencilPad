using System.Collections;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class EditablePolygonList : IEditablePolygonSet
{
    public IHandleSource HandleSource => _handleSource;

    public Unit2D Position
    {
        get => _handleSource.Position;
        set => _handleSource.Position = value;
    }
    
    private List<EditablePolygon> _polygons;
    private GroupHandleSource _handleSource;
    
    public EditablePolygon this[int index] => _polygons[index];
    public int Count => _polygons.Count;

    public event Action<EditablePolygon>? PolygonAdded;
    public event Action<EditablePolygon>? PolygonRemoved;
    
    public EditablePolygonList()
    {
        _polygons = [];
        _handleSource = new GroupHandleSource();
    }

    public void Add(EditablePolygon polygon)
    {
        _polygons.Add(polygon);
        _handleSource.Add(polygon);

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

        _handleSource.SetChildren(_polygons);
        _handleSource.Position = other._handleSource.Position;
        _handleSource.SetSelectedHandles(other.HandleSource.GetSelectedHandles());
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
