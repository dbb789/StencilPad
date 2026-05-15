namespace StencilPad.Spatial;

public class QuadTreeNode<T>
{
    public QuadTreeNode<T>? Parent => _parent;
    public bool IsLeaf => !_hasChildren;
    public bool IsEmpty => !_hasChildren && _values.Count == 0;
    public UnitBounds Bounds => _bounds;
    
    private readonly IObjectPool<QuadTreeNode<T>> _nodePool;
    private readonly int _nodeCapacity;
    private readonly List<(T, Unit2D)> _values;
    private QuadTreeNode<T>? _parent;
    private UnitBounds _bounds;
    private bool _hasChildren;
    private QuadTreeNodeSet<T> _children;
    private int _maxDepth;
    
    public QuadTreeNode(IObjectPool<QuadTreeNode<T>> nodePool,
                        int nodeCapacity)
    {
        _nodePool = nodePool;
        _bounds = UnitBounds.Empty;
        _nodeCapacity = nodeCapacity;
        _values = new(nodeCapacity + 1);
        _parent = null;
        _hasChildren = false;
        _maxDepth = 0;
    }

    public void Initialize(QuadTreeNode<T>? parent,
                           UnitBounds bounds,
                           int maxDepth)
    {
        Clear();
        
        _parent = parent;
        _bounds = bounds;
        _maxDepth = maxDepth;
    }

    public void Clear()
    {
        _parent = null;
        _values.Clear();
        
        if (_hasChildren)
        {
            _children.Recycle();
            _hasChildren = false;
        }
    }

    public void Insert(Unit2D point, T value)
    {
        if (_hasChildren)
        {
            _children.Insert(point, value);
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

    public QuadTreeNode<T>? Remove(UnitBounds bounds, T value)
    {
        if (!Bounds.Intersects(bounds))
        {
            return null;
        }

        if (_hasChildren)
        {
            return _children.Remove(bounds, value);
        }
        else
        {
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (EqualityComparer<T>.Default.Equals(_values[i].Item1, value) &&
                    bounds.Contains(_values[i].Item2))
                {
                    _values.RemoveAt(i);
                    
                    return this;
                }
            }
        }

        return null;
    }

    public void Prune()
    {
        if (_hasChildren && _children.Empty())
        {
            _children.Recycle();
            _hasChildren = false;
        }
    }

    public void Query(UnitBounds bounds, List<T> results)
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
        
        if (_hasChildren)
        {
            _children.Query(bounds, results);
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

    public void GetAllValues(List<T> results)
    {
        if (_hasChildren)
        {
            _children.GetAllValues(results);
        }
        else
        {
            foreach (var (value, valuePoint) in _values)
            {
                results.Add(value);
            }
        }
    }
    
    public void VisitAllValues(Action<Unit2D, T> func)
    {
        if (_hasChildren)
        {
            _children.VisitAllValues(func);
        }
        else
        {
            foreach (var entry in _values)
            {
                func(entry.Item2, entry.Item1);
            }
        }
    }

    private void Subdivide()
    {
        _children.Initialize(this,
                             _nodePool,
                             _nodeCapacity,
                             _bounds,
                             _maxDepth - 1);
        
        _hasChildren = true;

        foreach (var (value, valuePoint) in _values)
        {
            _children.Insert(valuePoint, value);
        }

        _values.Clear();
    }
}
