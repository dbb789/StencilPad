namespace StencilPad.Spatial;

public class DynamicQuadTree<T> where T : notnull
{
    private readonly IObjectPool<QuadTreeNode<T>> _nodePool;
    private UnitBounds _maxBounds;
    private QuadTree<T> _tree;
    private int _nodeCapacity;
    private int _maxDepth;

    public DynamicQuadTree(UnitBounds maxBounds,
                           UnitBounds initialBounds,
                           int nodeCapacity,
                           int maxDepth)
    {
        _nodePool = new ObjectPool<QuadTreeNode<T>>(256);
        _maxBounds = maxBounds;
        _nodeCapacity = nodeCapacity;
        _maxDepth = maxDepth;
        _tree = new QuadTree<T>(_nodePool,
                                initialBounds,
                                nodeCapacity,
                                maxDepth);
    }

    public bool Insert(Unit2D point, T value)
    {
        if (!SizeToFitPoint(point))
        {
            return false;
        }
        
        _tree.Insert(point, value);

        return true;
    }

    public bool Remove(T value)
    {
        return _tree.Remove(value);
    }

    public bool Move(Unit2D newPoint, T value)
    {
        if (!SizeToFitPoint(newPoint))
        {
            return false;
        }

        return _tree.Move(newPoint, value);
    }

    public void Clear()
    {
        _tree.Clear();
    }
    
    public void Query(UnitBounds bounds, Action<T> func)
    {
        _tree.Query(bounds, func);
    }

    public void VisitAllValues(Action<Unit2D, T> func)
    {
        _tree.VisitAllValues(func);
    }

    private bool SizeToFitPoint(Unit2D point)
    {
        var treeBounds = _tree.Bounds;

        if (!treeBounds.Contains(point))
        {
            if (!_maxBounds.Contains(point))
            {
                return false;
            }
            
            var nextBounds = treeBounds;

            while (!nextBounds.Contains(point))
            {
                nextBounds = UnitBounds.FromCenterSize(nextBounds.Center, nextBounds.Size * 2);
            }

            GrowTree(nextBounds);
        }

        return true;
    }
    
    private void GrowTree(UnitBounds newBounds)
    {
        var valueList = new List<(Unit2D, T)>(128);

        _tree.VisitAllValues((point, value) =>
        {
            valueList.Add((point, value));
        });

        _tree.Dispose();
        
        _tree = new QuadTree<T>(_nodePool,
                                newBounds,
                                _nodeCapacity,
                                _maxDepth);

        foreach (var (point, value) in valueList)
        {
            _tree.Insert(point, value);
        }
    }
}
