namespace StencilPad.Spatial;

public class QuadTree<T> : IDisposable where T : notnull
{
    private readonly IObjectPool<QuadTreeNode<T>> _nodePool;
    private readonly QuadTreeNode<T> _root;
    private readonly Dictionary<T, QuadTreeNode<T>> _lookup;

    public UnitBounds Bounds => _root.Bounds;
    
    public QuadTree(IObjectPool<QuadTreeNode<T>> nodePool,
                    UnitBounds bounds,
                    int nodeCapacity,
                    int maxDepth)
    {
        _nodePool = nodePool;
        _root = new QuadTreeNode<T>(nodePool, nodeCapacity);
        _root.Initialize(null, bounds, maxDepth);
        _lookup = new();
    }

    public void Dispose()
    {
        _root.Clear();
        _lookup.Clear();
    }

    public void Insert(Unit2D point, T value)
    {
        _root.Insert(point, value, _lookup);
    }

    public bool Remove(T value)
    {
        if (!_lookup.TryGetValue(value, out var node))
        {
            return false;
        }

        node.RemoveDirect(value);
        _lookup.Remove(value);
        node.Parent?.Prune();

        return true;
    }

    public bool Move(Unit2D newPoint, T value)
    {
        if (!_lookup.TryGetValue(value, out var node))
        {
            return false;
        }

        node.RemoveDirect(value);
        _lookup.Remove(value);

        var insertNode = node;

        while (insertNode.Parent is not null &&
               !insertNode.Bounds.Contains(newPoint))
        {
            insertNode = insertNode.Parent;
        }

        insertNode.Insert(newPoint, value, _lookup);
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
