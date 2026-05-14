namespace StencilPad.Spatial;

public struct QuadTreeNodeSet<T>
{
    private QuadTreeNode<T> _nw;
    private QuadTreeNode<T> _ne;
    private QuadTreeNode<T> _sw;
    private QuadTreeNode<T> _se;

    private UnitBounds _bounds;

    public QuadTreeNodeSet(UnitBounds bounds,
                           int nodeCapacity,
                           int maxDepth)
    {
        _bounds = bounds;
        
        _nw = new QuadTreeNode<T>(NWBounds(), nodeCapacity, maxDepth);
        _ne = new QuadTreeNode<T>(NEBounds(), nodeCapacity, maxDepth);
        _sw = new QuadTreeNode<T>(SWBounds(), nodeCapacity, maxDepth);
        _se = new QuadTreeNode<T>(SEBounds(), nodeCapacity, maxDepth);
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

    public void Remove(UnitBounds bounds, T value)
    {
        _nw.Remove(bounds, value);
        _ne.Remove(bounds, value);
        _sw.Remove(bounds, value);
        _se.Remove(bounds, value);
    }
    
    public void Query(UnitBounds bounds, List<T> results)
    {
        _nw.Query(bounds, results);
        _ne.Query(bounds, results);
        _sw.Query(bounds, results);
        _se.Query(bounds, results);
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
