namespace StencilPad.Spatial;

public class QuadTree<T>
{
    private static readonly Unit2D RemoveRegion = new(Unit.FromMillimeters(0.0001),
                                                      Unit.FromMillimeters(0.0001));
    
    private readonly QuadTreeNode<T> _root;
    
    public QuadTree(UnitBounds bounds,
                    int nodeCapacity,
                    int maxDepth)
    {
        _root = new QuadTreeNode<T>(bounds,
                                    nodeCapacity,
                                    maxDepth);
    }

    public void Insert(Unit2D point, T value)
    {
        _root.Insert(point, value);
    }

    public void Remove(Unit2D point, T value)
    {
        _root.Remove(UnitBounds.FromCenterSize(point, RemoveRegion), value);
    }

    public void Query(UnitBounds bounds, List<(T, Unit2D)> results)
    {
        _root.Query(bounds, results);
    }
}
