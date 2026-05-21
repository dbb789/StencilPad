namespace StencilPad.Tests.Spatial;

using StencilPad.Spatial;

public class DynamicQuadTreeTests
{
    private static Unit U(double v) => Unit.FromMillimeters(v);
    private static Unit2D U2(double x, double y) => new(U(x), U(y));
    private static UnitBounds Bounds(double cx, double cy, double w, double h) =>
        UnitBounds.FromCenterSize(U2(cx, cy), U2(w, h));

    private static DynamicQuadTree<T> MakeTree<T>(
        double maxSize = 10000,
        double initialSize = 100) where T : notnull
    {
        return new DynamicQuadTree<T>(
            Bounds(0, 0, maxSize, maxSize),
            Bounds(0, 0, initialSize, initialSize),
            nodeCapacity: 4,
            maxDepth: 4);
    }

    [Test]
    public void Insert_WithinInitialBounds_ReturnsTrueAndQueryFindsIt()
    {
        var tree = MakeTree<string>();

        var inserted = tree.Insert(U2(10, 10), "a");

        var results = new List<string>();
        tree.Query(Bounds(0, 0, 100, 100), results.Add);

        Assert.Multiple(() =>
        {
            Assert.That(inserted, Is.True);
            Assert.That(results, Does.Contain("a"));
        });
    }

    [Test]
    public void Insert_OutsideMaxBounds_ReturnsFalse()
    {
        var tree = MakeTree<string>(maxSize: 100);

        Assert.That(tree.Insert(U2(200, 200), "a"), Is.False);
    }

    [Test]
    public void Insert_OutsideInitialBoundsButWithinMax_GrowsTreeAndFindsValue()
    {
        var tree = MakeTree<string>(maxSize: 10000, initialSize: 100);

        var inserted = tree.Insert(U2(200, 200), "far");

        var results = new List<string>();
        tree.Query(Bounds(200, 200, 5, 5), results.Add);

        Assert.Multiple(() =>
        {
            Assert.That(inserted, Is.True);
            Assert.That(results, Does.Contain("far"));
        });
    }

    [Test]
    public void Insert_GrowsTree_PreservesExistingValues()
    {
        var tree = MakeTree<string>(maxSize: 10000, initialSize: 100);
        tree.Insert(U2(10, 10), "existing");

        tree.Insert(U2(200, 200), "far");

        var results = new List<string>();
        tree.Query(Bounds(0, 0, 10000, 10000), results.Add);

        Assert.That(results, Is.EquivalentTo(new[] { "existing", "far" }));
    }

    [Test]
    public void Remove_ExistingValue_ReturnsTrueAndQueryDoesNotFindIt()
    {
        var tree = MakeTree<string>();
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
        var tree = MakeTree<string>();

        Assert.That(tree.Remove("missing"), Is.False);
    }

    [Test]
    public void Move_ExistingValue_FindableAtNewPositionOnly()
    {
        var tree = MakeTree<string>();
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
            Assert.That(newResults, Does.Contain("a"));
        });
    }

    [Test]
    public void Move_OutsideMaxBounds_ReturnsFalse()
    {
        var tree = MakeTree<string>(maxSize: 100);
        tree.Insert(U2(10, 10), "a");

        Assert.That(tree.Move(U2(200, 200), "a"), Is.False);
    }

    [Test]
    public void Move_OutsideInitialBoundsButWithinMax_GrowsTreeAndUpdatesPosition()
    {
        var tree = MakeTree<string>(maxSize: 10000, initialSize: 100);
        tree.Insert(U2(10, 10), "a");

        var moved = tree.Move(U2(300, 300), "a");

        var oldResults = new List<string>();
        tree.Query(Bounds(10, 10, 5, 5), oldResults.Add);

        var newResults = new List<string>();
        tree.Query(Bounds(300, 300, 5, 5), newResults.Add);

        Assert.Multiple(() =>
        {
            Assert.That(moved, Is.True);
            Assert.That(oldResults, Is.Empty);
            Assert.That(newResults, Does.Contain("a"));
        });
    }

    [Test]
    public void Query_NothingInBounds_ReturnsEmpty()
    {
        var tree = MakeTree<string>();
        tree.Insert(U2(30, 30), "a");

        var results = new List<string>();
        tree.Query(Bounds(-20, -20, 5, 5), results.Add);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void VisitAllValues_ReturnsAllInsertedValuesWithCorrectPoints()
    {
        var tree = MakeTree<string>();
        tree.Insert(U2(-10, -10), "a");
        tree.Insert(U2(10, 10), "b");
        tree.Insert(U2(-10, 10), "c");

        var visited = new List<(Unit2D point, string value)>();
        tree.VisitAllValues((p, v) => visited.Add((p, v)));

        Assert.Multiple(() =>
        {
            Assert.That(visited.Select(x => x.value), Is.EquivalentTo(new[] { "a", "b", "c" }));
            Assert.That(visited.First(x => x.value == "a").point, Is.EqualTo(U2(-10, -10)));
            Assert.That(visited.First(x => x.value == "b").point, Is.EqualTo(U2(10, 10)));
        });
    }

    [Test]
    public void VisitAllValues_AfterGrow_ReturnsAllValues()
    {
        var tree = MakeTree<string>(maxSize: 10000, initialSize: 100);
        tree.Insert(U2(10, 10), "a");
        tree.Insert(U2(20, 20), "b");
        tree.Insert(U2(200, 200), "c"); // triggers grow

        var visited = new List<string>();
        tree.VisitAllValues((_, v) => visited.Add(v));

        Assert.That(visited, Is.EquivalentTo(new[] { "a", "b", "c" }));
    }
}
