namespace StencilPad.Spatial;

public class QuadTreeNode<T>
{
    public bool IsLeaf => _children == null;
    public bool IsEmpty => IsLeaf && _values.Count == 0;
    public UnitBounds Bounds => _bounds;
    
    private UnitBounds _bounds;
    private int _nodeCapacity;
    private List<(T, Unit2D)> _values;
    private QuadTreeNodeSet<T>? _children;
    private int _maxDepth;
    
    public QuadTreeNode(UnitBounds bounds,
                        int nodeCapacity,
                        int maxDepth)
    {
        _bounds = bounds;
        _nodeCapacity = nodeCapacity;
        _values = new(nodeCapacity + 1);
        _children = null;
        _maxDepth = maxDepth;
    }

    public void Insert(Unit2D point, T value)
    {
        if (_children is not null)
        {
            _children.Value.Insert(point, value);
        }
        else
        {
            _values.Add((value, point));
            
            if (_maxDepth > 0 && _values.Count > _nodeCapacity)
            {
                Subdivide();
            }
        }
    }

    public void Remove(UnitBounds bounds, T value)
    {
        if (!Bounds.Intersects(bounds))
        {
            return;
        }

        if (_children is not null)
        {
            _children.Value.Remove(bounds, value);

            if (_children.Value.Empty())
            {
                _children = null;
            }
        }
        else
        {
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (EqualityComparer<T>.Default.Equals(_values[i].Item1, value) &&
                    bounds.Contains(_values[i].Item2))
                {
                    _values.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public void Query(UnitBounds bounds, List<T> results)
    {
        if (!Bounds.Intersects(bounds))
        {
            return;
        }
        
        if (_children is not null)
        {
            _children.Value.Query(bounds, results);
        }
        else
        {
            foreach (var (value, valuePoint) in _values)
            {
                if (bounds.Contains(valuePoint))
                {
                    results.Add(value);
                }
            }
        }
    }

    private void Subdivide()
    {
        _children = new QuadTreeNodeSet<T>(_bounds, _nodeCapacity, _maxDepth - 1);
        
        foreach (var (value, valuePoint) in _values)
        {
            _children.Value.Insert(valuePoint, value);
        }

        _values.Clear();
    }
}
