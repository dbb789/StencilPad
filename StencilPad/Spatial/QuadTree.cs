namespace StencilPad.Spatial;

public class QuadTree<T> : IDisposable
{
    // Allows for numerical instability when removing points from the quadtree,
    // for example if a point is sitting on the edge of a node or is otherwise
    // offset by a small margin. Note that Remove() only ever removes up to one
    // element, so this shouldn't cause any real issues with inconsistency.
    private static readonly Unit2D SearchRegion = new(Unit.FromMillimeters(0.0001),
                                                      Unit.FromMillimeters(0.0001));

    private readonly IObjectPool<QuadTreeNode<T>> _nodePool;
    private readonly QuadTreeNode<T> _root;

    public UnitBounds Bounds => _root.Bounds;
    
    public QuadTree(IObjectPool<QuadTreeNode<T>> nodePool,
                    UnitBounds bounds,
                    int nodeCapacity,
                    int maxDepth)
    {
        _nodePool = nodePool;
        _root = new QuadTreeNode<T>(nodePool, nodeCapacity);
        _root.Initialize(null, bounds, maxDepth);
    }

    public void Dispose()
    {
        _root.Clear();
    }

    public void Insert(Unit2D point, T value)
    {
        _root.Insert(point, value);
    }

    public bool Remove(Unit2D point, T value)
    {
        var node = _root.Remove(UnitBounds.FromCenterSize(point, SearchRegion), value);

        node?.Parent?.Prune();

        return node is not null;
    }

    public bool Move(Unit2D oldPoint, Unit2D newPoint, T value)
    {
        var node = _root.Remove(UnitBounds.FromCenterSize(oldPoint, SearchRegion), value);

        if (node is null)
        {
            return false;
        }

        var insertNode = node;

        while (insertNode.Parent is not null &&
               !insertNode.Bounds.Contains(newPoint))
        {
            insertNode = insertNode.Parent;
        }

        insertNode.Insert(newPoint, value);
        node.Parent?.Prune();

        return true;
    }

    public void Query(UnitBounds bounds, Action<T> func)
    {
        _root.Query(bounds, func);
    }

    public void VisitAllValues(Action<Unit2D, T> func)
    {
        _root.VisitAllValues(func);
    }
}
