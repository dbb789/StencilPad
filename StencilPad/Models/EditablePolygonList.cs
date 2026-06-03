using System.Collections;
using StencilPad.Spatial;

namespace StencilPad.Models;

public class EditablePolygonList : IEditablePolygonSet
{
    public IHandleSource HandleSource => _handleSource;

    private List<EditablePolygon> _polygons;
    private GroupHandleSource<IHandleSource> _handleSource;
    
    public EditablePolygon this[int index] => _polygons[index];
    public int Count => _polygons.Count;

    public event Action<EditablePolygon>? PolygonAdded;
    public event Action<EditablePolygon>? PolygonRemoved;
    
    public EditablePolygonList()
    {
        _polygons = [];
        _handleSource = new();
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
        // Reuse existing objects where possible to avoid additional heap allocations.

        // First, if we've got more polygons than our target, chop them out.
        while (_polygons.Count > other._polygons.Count)
        {
            Remove(_polygons[^1]);
        }

        // Now copy the polygons from our target into our existing polygons.
        for (int i = 0; i < _polygons.Count; ++i)
        {
            _polygons[i].AssignFrom(other._polygons[i]);
        }

        // Add any additional polygons from our target that we don't have yet.
        for (int i = _polygons.Count; i < other._polygons.Count; ++i)
        {
            var polygon = other._polygons[i].DeepClone();
            
            _polygons.Add(polygon);
            PolygonAdded?.Invoke(polygon);
        }

        // Finally update the handle source to match our new list of polygons.
        _handleSource.SetChildren(_polygons);
    }

    public UnitBounds CalculateBounds()
    {
        return CalculateBounds(UnitTransform.Identity);
    }
    
    public UnitBounds CalculateBounds(UnitTransform transform)
    {
        UnitBounds? bounds = null;

        foreach (var polygon in _polygons)
        {
            bounds = UnitBounds.Union(bounds, polygon.CalculateBounds(transform));
        }

        return bounds ?? UnitBounds.Empty;
    }

    public void SetBounds(UnitBounds newBounds, UnitTransform transform)
    {
        var oldBounds = CalculateBounds(transform);

        foreach (var polygon in _polygons)
        {
            polygon.SetBounds(oldBounds, newBounds, transform);
        }
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

    public bool ContainsPoint(Unit2D point, Unit tolerance)
    {
        foreach (var polygon in _polygons)
        {
            if (PolygonUtil.ContainsPoint(polygon, point, tolerance))
            {
                return true;
            }
        }

        return false;
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
