namespace StencilPad.Spatial;

public class DynamicQuadTree<T>
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

    public bool Remove(Unit2D point, T value)
    {
        return _tree.Remove(point, value);
    }

    public bool Move(Unit2D oldPoint, Unit2D newPoint, T value)
    {
        if (!SizeToFitPoint(newPoint))
        {
            return false;
        }

        return _tree.Move(oldPoint, newPoint, value);
    }

    public void Query(UnitBounds bounds, List<T> results)
    {
        _tree.Query(bounds, results);
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
        var newTree = new QuadTree<T>(_nodePool,
                                      newBounds,
                                      _nodeCapacity,
                                      _maxDepth);

        _tree.VisitAllValues((point, value) =>
        {
            newTree.Insert(point, value);
        });

        _tree.Dispose();
        _tree = newTree;
    }
}
