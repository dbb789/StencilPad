using StencilPad.Collections;

namespace StencilPad.Spatial;

public class QuadTreeNode<T> where T : notnull
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

    public void Insert(Unit2D point, T value, Dictionary<T, QuadTreeNode<T>> lookup)
    {
        if (_hasChildren)
        {
            _children.Insert(point, value, lookup);
        }
        else
        {
            _values.Add((value, point));
            lookup[value] = this;
            
            if (_maxDepth > 0 && _values.Count > _nodeCapacity)
            {
                Subdivide(lookup);
            }
        }
    }

    public void RemoveDirect(T value)
    {
        for (int i = _values.Count - 1; i >= 0; i--)
        {
            if (EqualityComparer<T>.Default.Equals(_values[i].Item1, value))
            {
                _values.RemoveAt(i);
                return;
            }
        }
    }

    public void Prune()
    {
        if (!_hasChildren || !_children.Empty())
        {
            return;
        }

        _children.Recycle();
        _hasChildren = false;
        _parent?.Prune();
    }

    public void Query(UnitBounds bounds, Action<T> func)
    {
        if (!Bounds.Intersects(bounds))
        {
            return;
        }

        // If this node is completely within the query bounds, we can add all of
        // its values without further checks.
        if (bounds.Contains(Bounds))
        {
            GetAllValues(func);
            return;
        }
        
        if (_hasChildren)
        {
            _children.Query(bounds, func);
        }
        else
        {
            foreach (var (value, valuePoint) in _values)
            {
                if (bounds.Contains(valuePoint))
                {
                    func(value);
                }
            }
        }
    }

    public void GetAllValues(Action<T> func)
    {
        if (_hasChildren)
        {
            _children.GetAllValues(func);
        }
        else
        {
            foreach (var (value, valuePoint) in _values)
            {
                func(value);
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

    private void Subdivide(Dictionary<T, QuadTreeNode<T>> lookup)
    {
        _children.Initialize(this,
                             _nodePool,
                             _nodeCapacity,
                             _bounds,
                             _maxDepth - 1);
        
        _hasChildren = true;

        foreach (var (value, valuePoint) in _values)
        {
            _children.Insert(valuePoint, value, lookup);
        }

        _values.Clear();
    }
}
