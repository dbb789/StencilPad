namespace StencilPad.Tests.Collections;

using StencilPad.Collections;
using StencilPad.Spatial;

public class QuadTreeTests
{
    private static Unit U(double v) => Unit.FromMillimeters(v);
    private static Unit2D U2(double x, double y) => new(U(x), U(y));
    private static UnitBounds Bounds(double cx, double cy, double w, double h) =>
        UnitBounds.FromCenterSize(U2(cx, cy), U2(w, h));

    private static QuadTree<T> MakeTree<T>(int nodeCapacity = 4, int maxDepth = 4) where T : notnull
    {
        var pool = new ObjectPool<QuadTreeNode<T>>(32);
        return new QuadTree<T>(pool, Bounds(0, 0, 100, 100), nodeCapacity, maxDepth);
    }

    [Test]
    public void Insert_SingleValue_QueryFindsIt()
    {
        using var tree = MakeTree<string>();
        tree.Insert(U2(10, 10), "a");

        var results = new List<string>();
        tree.Query(Bounds(0, 0, 100, 100), results.Add);

        Assert.That(results, Is.EqualTo(new[] { "a" }));
    }

    [Test]
    public void Insert_MultipleValues_QueryFindsAll()
    {
        using var tree = MakeTree<string>();
        tree.Insert(U2(-20, -20), "a");
        tree.Insert(U2(20, 20), "b");
        tree.Insert(U2(-20, 20), "c");

        var results = new List<string>();
        tree.Query(Bounds(0, 0, 100, 100), results.Add);

        Assert.That(results, Is.EquivalentTo(new[] { "a", "b", "c" }));
    }

    [Test]
    public void Query_NonIntersectingBounds_ReturnsEmpty()
    {
        using var tree = MakeTree<string>();
        tree.Insert(U2(30, 30), "a");

        var results = new List<string>();
        tree.Query(Bounds(-20, -20, 5, 5), results.Add);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Query_PartialBounds_FindsOnlyValuesInRegion()
    {
        using var tree = MakeTree<string>(nodeCapacity: 2);
        tree.Insert(U2(-30, -30), "sw");
        tree.Insert(U2(30, -30), "se");
        tree.Insert(U2(-30, 30), "nw");
        tree.Insert(U2(30, 30), "ne");

        var results = new List<string>();
        tree.Query(Bounds(-25, 0, 50, 100), results.Add); // left half: [-50,-50] to [0,50]

        Assert.Multiple(() =>
        {
            Assert.That(results, Does.Contain("sw").And.Contain("nw"));
            Assert.That(results, Does.Not.Contain("se").And.Not.Contain("ne"));
        });
    }

    [Test]
    public void Remove_ExistingValue_ReturnsTrueAndQueryDoesNotFindIt()
    {
        using var tree = MakeTree<string>();
        tree.Insert(U2(10, 10), "a");

        var removed = tree.Remove("a");

        var results = new List<string>();
        tree.Query(Bounds(0, 0, 100, 100), results.Add);

        Assert.Multiple(() =>
        {
            Assert.That(removed, Is.True);
            Assert.That(results, Is.Empty);
        });
    }

    [Test]
    public void Remove_NonExistentValue_ReturnsFalse()
    {
        using var tree = MakeTree<string>();

        Assert.That(tree.Remove("missing"), Is.False);
    }

    [Test]
    public void Move_ExistingValue_FindableAtNewPositionOnly()
    {
        using var tree = MakeTree<string>();
        tree.Insert(U2(-20, -20), "a");

        var moved = tree.Move(U2(20, 20), "a");

        var oldResults = new List<string>();
        tree.Query(Bounds(-20, -20, 5, 5), oldResults.Add);

        var newResults = new List<string>();
        tree.Query(Bounds(20, 20, 5, 5), newResults.Add);

        Assert.Multiple(() =>
        {
            Assert.That(moved, Is.True);
            Assert.That(oldResults, Is.Empty);
            Assert.That(newResults, Is.EqualTo(new[] { "a" }));
        });
    }

    [Test]
    public void Move_NonExistentValue_ReturnsFalse()
    {
        using var tree = MakeTree<string>();

        Assert.That(tree.Move(U2(10, 10), "missing"), Is.False);
    }

    [Test]
    public void Insert_ExceedingNodeCapacity_SubdividesAndAllValuesStillFound()
    {
        using var tree = MakeTree<string>(nodeCapacity: 2);
        tree.Insert(U2(-30, -30), "a");
        tree.Insert(U2(30, -30), "b");
        tree.Insert(U2(-30, 30), "c");
        tree.Insert(U2(30, 30), "d");
        tree.Insert(U2(0, 0), "e");

        var results = new List<string>();
        tree.Query(Bounds(0, 0, 100, 100), results.Add);

        Assert.That(results, Is.EquivalentTo(new[] { "a", "b", "c", "d", "e" }));
    }

    [Test]
    public void Remove_AfterSubdivide_CorrectlyRemovesValue()
    {
        using var tree = MakeTree<string>(nodeCapacity: 2);
        tree.Insert(U2(-30, -30), "a");
        tree.Insert(U2(30, -30), "b");
        tree.Insert(U2(-30, 30), "c");
        tree.Insert(U2(30, 30), "d");

        tree.Remove("b");

        var results = new List<string>();
        tree.Query(Bounds(0, 0, 100, 100), results.Add);

        Assert.Multiple(() =>
        {
            Assert.That(results, Does.Not.Contain("b"));
            Assert.That(results, Is.EquivalentTo(new[] { "a", "c", "d" }));
        });
    }

    [Test]
    public void Move_AfterSubdivide_UpdatesPositionCorrectly()
    {
        using var tree = MakeTree<string>(nodeCapacity: 2);
        tree.Insert(U2(-30, -30), "a");
        tree.Insert(U2(30, -30), "b");
        tree.Insert(U2(-30, 30), "c");
        tree.Insert(U2(30, 30), "d");

        tree.Move(U2(5, 5), "a");

        var oldResults = new List<string>();
        tree.Query(Bounds(-30, -30, 5, 5), oldResults.Add);

        var newResults = new List<string>();
        tree.Query(Bounds(5, 5, 5, 5), newResults.Add);

        Assert.Multiple(() =>
        {
            Assert.That(oldResults, Does.Not.Contain("a"));
            Assert.That(newResults, Does.Contain("a"));
        });
    }

    [Test]
    public void VisitAllValues_ReturnsAllValuesWithCorrectPoints()
    {
        using var tree = MakeTree<string>();
        tree.Insert(U2(-20, -20), "a");
        tree.Insert(U2(20, 20), "b");
        tree.Insert(U2(-20, 20), "c");

        var visited = new List<(Unit2D point, string value)>();
        tree.VisitAllValues((p, v) => visited.Add((p, v)));

        Assert.Multiple(() =>
        {
            Assert.That(visited.Select(x => x.value), Is.EquivalentTo(new[] { "a", "b", "c" }));
            Assert.That(visited.First(x => x.value == "a").point, Is.EqualTo(U2(-20, -20)));
            Assert.That(visited.First(x => x.value == "b").point, Is.EqualTo(U2(20, 20)));
            Assert.That(visited.First(x => x.value == "c").point, Is.EqualTo(U2(-20, 20)));
        });
    }

    [Test]
    public void Dispose_WithValues_DoesNotThrow()
    {
        var pool = new ObjectPool<QuadTreeNode<string>>(32);
        var tree = new QuadTree<string>(pool, Bounds(0, 0, 100, 100), 4, 4);
        tree.Insert(U2(10, 10), "a");
        tree.Insert(U2(-10, -10), "b");

        Assert.That(() => tree.Dispose(), Throws.Nothing);
    }
    private class TrackingNodePool<TNode> : IObjectPool<QuadTreeNode<TNode>> where TNode : notnull
    {
        private readonly int _nodeCapacity;
        public int ActiveNodes { get; private set; }
        public int TotalAllocated { get; private set; }

        public TrackingNodePool(int nodeCapacity)
        {
            _nodeCapacity = nodeCapacity;
        }

        public QuadTreeNode<TNode>? TryGet()
        {
            ActiveNodes++;
            TotalAllocated++;
            return new QuadTreeNode<TNode>(this, _nodeCapacity); 
        }

        public void Recycle(QuadTreeNode<TNode> obj)
        {
            ActiveNodes--;
        }
    }

    [Test]
    public void StressTest_MassiveInsertAndRemove_LeavesZeroDanglingNodes()
    {
        int nodeCapacity = 4;
        var trackingPool = new TrackingNodePool<string>(nodeCapacity);
        
        using var tree = new QuadTree<string>(trackingPool, Bounds(0, 0, 1000, 1000), nodeCapacity, maxDepth: 8);

        var random = new Random(42); // Fixed seed for reproducibility
        int elementCount = 10000;
        var elements = new List<(string id, Unit2D point)>();

        // 1. Insert a massive number of randomly generated elements
        for (int i = 0; i < elementCount; i++)
        {
            string id = $"item_{i}";
            // Generate points within [-500, 500]
            double x = (random.NextDouble() * 1000) - 500;
            double y = (random.NextDouble() * 1000) - 500;
            var point = U2(x, y);

            elements.Add((id, point));
            tree.Insert(point, id);
        }

        // 2. Verify significant churn occurred
        Assert.That(trackingPool.TotalAllocated, Is.GreaterThan(1000), 
            "Test did not generate enough subdivisions to be a meaningful stress test.");

        // 3. Verify tree holds all elements
        var results = new List<string>();
        tree.Query(Bounds(0, 0, 1000, 1000), results.Add);
        Assert.That(results.Count, Is.EqualTo(elementCount));

        // 3.5 Verify each element can be found using a very tight bounding box
        foreach (var (id, point) in elements)
        {
            var tinyBounds = UnitBounds.FromCenterSize(point, U2(0.002, 0.002));
            var localResults = new List<string>();
            tree.Query(tinyBounds, localResults.Add);
            
            Assert.That(localResults, Does.Contain(id), 
                $"Element {id} at {point.X.Millimeters}, {point.Y.Millimeters} was not found by a localized query!");
        }

        // 4. Remove all elements in a random order and verify they are un-queryable
        for (int i = elements.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (elements[i], elements[j]) = (elements[j], elements[i]);
        }

        foreach (var (id, point) in elements)
        {
            tree.Remove(id);

            var tinyBounds = UnitBounds.FromCenterSize(point, U2(0.002, 0.002));
            var localResults = new List<string>();
            tree.Query(tinyBounds, localResults.Add);

            Assert.That(localResults, Does.Not.Contain(id), 
                $"Element {id} was still found by a localized query after being removed!");
        }

        // 5. Verify the tree fully collapsed and recycled all dynamically allocated child nodes
        Assert.That(trackingPool.ActiveNodes, Is.EqualTo(0), 
            $"Not all child nodes were recycled after removing all elements! ({trackingPool.ActiveNodes} left dangling)");
    }
}
