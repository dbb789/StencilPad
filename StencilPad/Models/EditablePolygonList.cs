using System.Collections;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class EditablePolygonList : IEditablePolygonSet
{
    public IHandleSource HandleSource => _handleSource;

    private List<EditablePolygon> _polygons;
    private PolygonListHandleSource _handleSource;
    
    public EditablePolygon this[int index] => _polygons[index];
    public int Count => _polygons.Count;

    public event Action<EditablePolygon>? PolygonAdded;
    public event Action<EditablePolygon>? PolygonRemoved;
    
    public EditablePolygonList()
    {
        _polygons = [];
        _handleSource = new PolygonListHandleSource();
    }

    public void Add(EditablePolygon polygon)
    {
        _polygons.Add(polygon);
        _handleSource.Add(polygon);

        PolygonAdded?.Invoke(polygon);
    }

    public void Remove(EditablePolygon polygon)
    {
        if (_polygons.Remove(polygon))
        {
            _handleSource.Remove(polygon);
            PolygonRemoved?.Invoke(polygon);
        }
    }

    public void Clear()
    {
        for (int i = _polygons.Count - 1; i >= 0; --i)
        {
            Remove(_polygons[i]);
        }
    }
    
    public void AssignFrom(EditablePolygonList other)
    {
        Clear();

        int polygonCount = other._polygons.Count;
        
        _polygons.Capacity = polygonCount;

        for (int i = 0; i < polygonCount; ++i)
        {
            var polygon = other._polygons[i].DeepClone();
            
            _polygons.Add(polygon);
            PolygonAdded?.Invoke(polygon);
        }

        _handleSource.SetChildren(_polygons);
    }

    public UnitBounds CalculateBounds()
    {
        UnitBounds? bounds = null;

        foreach (var polygon in _polygons)
        {
            bounds = UnitBounds.Union(bounds, polygon.CalculateBounds());
        }

        return bounds ?? UnitBounds.Empty;
    }

    public Unit2D CalculateMidpoint()
    {
        if (_polygons.Count == 0)
        {
            return Unit2D.Zero;
        }

        var sum = Unit2D.Zero;

        foreach (var polygon in _polygons)
        {
            sum += polygon.CalculateMidpoint();
        }

        return sum / _polygons.Count;
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
