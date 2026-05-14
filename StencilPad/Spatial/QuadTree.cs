namespace StencilPad.Spatial;

public class QuadTree<T>
{
    // Allows for numerical instability when removing points from the quadtree,
    // for example if a point is sitting on the edge of a node or is otherwise
    // offset by a small margin. Note that Remove() only ever removes up to one
    // element, so this shouldn't cause any real issues with inconsistency.
    private static readonly Unit2D RemoveRegion = new(Unit.FromMillimeters(0.0001),
                                                      Unit.FromMillimeters(0.0001));

    private readonly QuadTreeNode<T> _root;
    
    public QuadTree(UnitBounds bounds,
                    int nodeCapacity,
                    int maxDepth)
    {
        _root = new QuadTreeNode<T>(new ObjectPool<QuadTreeNode<T>>(256),
                                    nodeCapacity);
        
        _root.Initialize(bounds, maxDepth);
    }

    public void Insert(Unit2D point, T value)
    {
        _root.Insert(point, value);
    }

    public bool Remove(Unit2D point, T value)
    {
        return _root.Remove(UnitBounds.FromCenterSize(point, RemoveRegion), value);
    }

    public void Query(UnitBounds bounds, List<(T, Unit2D)> results)
    {
        _root.Query(bounds, results);
    }
}
