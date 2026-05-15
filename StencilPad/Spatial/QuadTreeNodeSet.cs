namespace StencilPad.Spatial;

public struct QuadTreeNodeSet<T>
{
    private IObjectPool<QuadTreeNode<T>> _nodePool;
    private UnitBounds _bounds;
    private QuadTreeNode<T> _nw;
    private QuadTreeNode<T> _ne;
    private QuadTreeNode<T> _sw;
    private QuadTreeNode<T> _se;


    public void Initialize(QuadTreeNode<T> parent,
                           IObjectPool<QuadTreeNode<T>> nodePool,
                           int nodeCapacity,
                           UnitBounds bounds,
                           int maxDepth)
    {
        _nodePool = nodePool;
        _bounds = bounds;
        _nw = nodePool.TryGet() ?? new QuadTreeNode<T>(nodePool, nodeCapacity);
        _nw.Initialize(parent, NWBounds(), maxDepth);
        
        _ne = nodePool.TryGet() ?? new QuadTreeNode<T>(nodePool, nodeCapacity);
        _ne.Initialize(parent, NEBounds(), maxDepth);
        
        _sw = nodePool.TryGet() ?? new QuadTreeNode<T>(nodePool, nodeCapacity);
        _sw.Initialize(parent, SWBounds(), maxDepth);
        
        _se = nodePool.TryGet() ?? new QuadTreeNode<T>(nodePool, nodeCapacity);
        _se.Initialize(parent, SEBounds(), maxDepth);
    }

    public void Recycle()
    {
        _nw.Clear();
        _nodePool.Recycle(_nw);
        _nw = null!;

        _ne.Clear();
        _nodePool.Recycle(_ne);
        _ne = null!;

        _sw.Clear();
        _nodePool.Recycle(_sw);
        _sw = null!;

        _se.Clear();
        _nodePool.Recycle(_se);
        _se = null!;
    }
    
    public void Insert(Unit2D point, T value)
    {
        if (point.X < _bounds.Center.X)
        {
            if (point.Y < _bounds.Center.Y)
            {
                _sw.Insert(point, value);
            }
            else
            {
                _nw.Insert(point, value);
            }
        }
        else
        {
            if (point.Y < _bounds.Center.Y)
            {
                _se.Insert(point, value);
            }
            else
            {
                _ne.Insert(point, value);
            }
        }
    }

    public QuadTreeNode<T>? Remove(UnitBounds bounds, T value)
    {
        return _nw.Remove(bounds, value)
            ?? _ne.Remove(bounds, value)
            ?? _sw.Remove(bounds, value)
            ?? _se.Remove(bounds, value);
    }
    
    public void Query(UnitBounds bounds, List<T> results)
    {
        _nw.Query(bounds, results);
        _ne.Query(bounds, results);
        _sw.Query(bounds, results);
        _se.Query(bounds, results);
    }
    
    public void GetAllValues(List<T> results)
    {
        _nw.GetAllValues(results);
        _ne.GetAllValues(results);
        _sw.GetAllValues(results);
        _se.GetAllValues(results);
    }
    
    public bool Empty()
    {
        return _nw.IsEmpty && _ne.IsEmpty && _sw.IsEmpty && _se.IsEmpty;
    }

    private UnitBounds NWBounds()
    {
        return UnitBounds.FromCenterSize(new Unit2D(_bounds.Center.X - _bounds.Size.X / 4,
                                                    _bounds.Center.Y + _bounds.Size.Y / 4),
                                         _bounds.Size / 2);
    }

    private UnitBounds NEBounds()
    {
        return UnitBounds.FromCenterSize(new Unit2D(_bounds.Center.X + _bounds.Size.X / 4,
                                                    _bounds.Center.Y + _bounds.Size.Y / 4),
                                         _bounds.Size / 2);
    }

    private UnitBounds SWBounds()
    {
        return UnitBounds.FromCenterSize(new Unit2D(_bounds.Center.X - _bounds.Size.X / 4,
                                                    _bounds.Center.Y - _bounds.Size.Y / 4),
                                         _bounds.Size / 2);
    }

    private UnitBounds SEBounds()
    {
        return UnitBounds.FromCenterSize(new Unit2D(_bounds.Center.X + _bounds.Size.X / 4,
                                                    _bounds.Center.Y - _bounds.Size.Y / 4),
                                         _bounds.Size / 2);
    }
}
