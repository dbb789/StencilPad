namespace StencilPad.Spatial;

public class QuadTreeNode<T>
{
    public bool IsLeaf => _children == null;
    public bool IsEmpty => IsLeaf && _values.Count == 0;
    public UnitBounds Bounds => _bounds;

    private readonly IObjectPool<QuadTreeNode<T>> _nodePool;
    private readonly int _nodeCapacity;
    private readonly List<(T, Unit2D)> _values;
    private UnitBounds _bounds;
    private QuadTreeNodeSet<T>? _children;
    private int _maxDepth;
    
    public QuadTreeNode(IObjectPool<QuadTreeNode<T>> nodePool,
                        int nodeCapacity)
    {
        _nodePool = nodePool;
        _bounds = UnitBounds.Empty;
        _nodeCapacity = nodeCapacity;
        _values = new(nodeCapacity + 1);
        _children = null;
        _maxDepth = 0;
    }

    public void Initialize(UnitBounds bounds, int maxDepth)
    {
        _bounds = bounds;
        _maxDepth = maxDepth;
        
        Clear();
    }

    public void Clear()
    {
        _values.Clear();
        
        if (_children is not null)
        {
            _children.Value.Recycle();
            _children = null;
        }
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

    public bool Remove(UnitBounds bounds, T value)
    {
        if (!Bounds.Intersects(bounds))
        {
            return false;
        }

        if (_children is not null)
        {
            bool removed = _children.Value.Remove(bounds, value);

            if (_children.Value.Empty())
            {
                _children.Value.Recycle();
                _children = null;
            }

            return removed;
        }
        else
        {
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (EqualityComparer<T>.Default.Equals(_values[i].Item1, value) &&
                    bounds.Contains(_values[i].Item2))
                {
                    _values.RemoveAt(i);
                    return true;
                }
            }
        }

        return false;
    }

    public void Query(UnitBounds bounds, List<(T, Unit2D)> results)
    {
        if (!Bounds.Intersects(bounds))
        {
            return;
        }

        // If this node is completely within the query bounds, we can add all of
        // its values without further checks.
        if (bounds.Contains(Bounds))
        {
            GetAllValues(results);
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
                    results.Add((value, valuePoint));
                }
            }
        }
    }

    public void GetAllValues(List<(T, Unit2D)> results)
    {
        if (_children is not null)
        {
            _children.Value.GetAllValues(results);
        }
        else
        {
            results.AddRange(_values);
        }
    }
    
    private void Subdivide()
    {
        _children = new QuadTreeNodeSet<T>(_nodePool,
                                           _nodeCapacity,
                                           _bounds,
                                           _maxDepth - 1);
        
        foreach (var (value, valuePoint) in _values)
        {
            _children.Value.Insert(valuePoint, value);
        }

        _values.Clear();
    }
}
