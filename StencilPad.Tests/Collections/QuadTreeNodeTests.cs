namespace StencilPad.Tests.Collections;

using StencilPad.Collections;
using StencilPad.Spatial;

public class QuadTreeNodeTests
{
    private static Unit U(double v) => Unit.FromMillimeters(v);
    private static Unit2D U2(double x, double y) => new(U(x), U(y));
    private static UnitBounds Bounds(double cx, double cy, double w, double h) =>
        UnitBounds.FromCenterSize(U2(cx, cy), U2(w, h));

    private static (QuadTreeNode<T> node, ObjectPool<QuadTreeNode<T>> pool)
        MakeNode<T>(int nodeCapacity = 4, int maxDepth = 4) where T : notnull
    {
        var pool = new ObjectPool<QuadTreeNode<T>>(32);
        var node = new QuadTreeNode<T>(pool, nodeCapacity);
        node.Initialize(null, Bounds(0, 0, 100, 100), maxDepth);
        return (node, pool);
    }

    // --- State ---

    [Test]
    public void InitialState_IsLeafAndEmpty()
    {
        var (node, _) = MakeNode<string>();

        Assert.Multiple(() =>
        {
            Assert.That(node.IsLeaf, Is.True);
            Assert.That(node.IsEmpty, Is.True);
            Assert.That(node.Parent, Is.Null);
        });
    }

    [Test]
    public void Insert_SingleValue_IsLeafAndNotEmpty()
    {
        var (node, _) = MakeNode<string>();
        var lookup = new Dictionary<string, QuadTreeNode<string>>();

        node.Insert(U2(10, 10), "a", lookup);

        Assert.Multiple(() =>
        {
            Assert.That(node.IsLeaf, Is.True);
            Assert.That(node.IsEmpty, Is.False);
        });
    }

    // --- Lookup insertion ---

    [Test]
    public void Insert_SingleValue_AddsToLookupPointingToThisNode()
    {
        var (node, _) = MakeNode<string>();
        var lookup = new Dictionary<string, QuadTreeNode<string>>();

        node.Insert(U2(10, 10), "a", lookup);

        Assert.Multiple(() =>
        {
            Assert.That(lookup, Does.ContainKey("a"));
            Assert.That(lookup["a"], Is.SameAs(node));
        });
    }

    [Test]
    public void Insert_MultipleValues_AllAddedToLookupPointingToThisNode()
    {
        var (node, _) = MakeNode<string>();
        var lookup = new Dictionary<string, QuadTreeNode<string>>();

        node.Insert(U2(-20, -20), "a", lookup);
        node.Insert(U2(20, 20), "b", lookup);
        node.Insert(U2(-20, 20), "c", lookup);

        Assert.Multiple(() =>
        {
            Assert.That(lookup["a"], Is.SameAs(node));
            Assert.That(lookup["b"], Is.SameAs(node));
            Assert.That(lookup["c"], Is.SameAs(node));
        });
    }

    // --- Subdivide and lookup migration ---

    [Test]
    public void Insert_ExceedingCapacity_TriggersSubdivide_NodeIsNoLongerLeaf()
    {
        var (node, _) = MakeNode<string>(nodeCapacity: 2);
        var lookup = new Dictionary<string, QuadTreeNode<string>>();

        node.Insert(U2(-20, -20), "a", lookup);
        node.Insert(U2(20, -20), "b", lookup);
        node.Insert(U2(-20, 20), "c", lookup); // exceeds capacity, triggers subdivide

        Assert.That(node.IsLeaf, Is.False);
    }

    [Test]
    public void Insert_ExceedingCapacity_LookupEntriesMigrateToChildNodes()
    {
        var (node, _) = MakeNode<string>(nodeCapacity: 2);
        var lookup = new Dictionary<string, QuadTreeNode<string>>();

        node.Insert(U2(-20, -20), "a", lookup);
        node.Insert(U2(20, -20), "b", lookup);
        node.Insert(U2(-20, 20), "c", lookup); // triggers subdivide

        // After subdivide, all entries must point to child nodes, not the root
        Assert.Multiple(() =>
        {
            Assert.That(lookup["a"], Is.Not.SameAs(node));
            Assert.That(lookup["b"], Is.Not.SameAs(node));
            Assert.That(lookup["c"], Is.Not.SameAs(node));
        });
    }

    [Test]
    public void Insert_ExceedingCapacity_LookupChildNodesHaveCorrectParent()
    {
        var (node, _) = MakeNode<string>(nodeCapacity: 2);
        var lookup = new Dictionary<string, QuadTreeNode<string>>();

        node.Insert(U2(-20, -20), "a", lookup);
        node.Insert(U2(20, -20), "b", lookup);
        node.Insert(U2(-20, 20), "c", lookup); // triggers subdivide

        Assert.Multiple(() =>
        {
            Assert.That(lookup["a"].Parent, Is.SameAs(node));
            Assert.That(lookup["b"].Parent, Is.SameAs(node));
            Assert.That(lookup["c"].Parent, Is.SameAs(node));
        });
    }

    [Test]
    public void Insert_ExceedingCapacity_ValuesInDifferentQuadrantsGoToDifferentChildNodes()
    {
        var (node, _) = MakeNode<string>(nodeCapacity: 2);
        var lookup = new Dictionary<string, QuadTreeNode<string>>();

        node.Insert(U2(-20, -20), "sw", lookup); // SW quadrant
        node.Insert(U2(20, -20), "se", lookup);  // SE quadrant
        node.Insert(U2(-20, 20), "nw", lookup);  // NW quadrant — triggers subdivide

        // Each value is in a different quadrant so each must be in a different child node
        Assert.Multiple(() =>
        {
            Assert.That(lookup["sw"], Is.Not.SameAs(lookup["se"]));
            Assert.That(lookup["sw"], Is.Not.SameAs(lookup["nw"]));
            Assert.That(lookup["se"], Is.Not.SameAs(lookup["nw"]));
        });
    }

    [Test]
    public void Insert_AtMaxDepthZero_NeverSubdivides_LookupPointsToRootNode()
    {
        var (node, _) = MakeNode<string>(nodeCapacity: 2, maxDepth: 0);
        var lookup = new Dictionary<string, QuadTreeNode<string>>();

        node.Insert(U2(-20, -20), "a", lookup);
        node.Insert(U2(20, -20), "b", lookup);
        node.Insert(U2(-20, 20), "c", lookup); // would subdivide, but maxDepth prevents it

        Assert.Multiple(() =>
        {
            Assert.That(node.IsLeaf, Is.True);
            Assert.That(lookup["a"], Is.SameAs(node));
            Assert.That(lookup["b"], Is.SameAs(node));
            Assert.That(lookup["c"], Is.SameAs(node));
        });
    }

    // --- RemoveDirect ---

    [Test]
    public void RemoveDirect_RemovesValueFromQuery_ButLeavesLookupUnchanged()
    {
        var (node, _) = MakeNode<string>();
        var lookup = new Dictionary<string, QuadTreeNode<string>>();
        node.Insert(U2(10, 10), "a", lookup);

        node.RemoveDirect("a");

        var results = new List<string>();
        node.Query(Bounds(0, 0, 100, 100), results.Add);

        Assert.Multiple(() =>
        {
            Assert.That(results, Is.Empty);
            Assert.That(lookup, Does.ContainKey("a")); // caller's responsibility to update lookup
        });
    }

    [Test]
    public void RemoveDirect_NonExistentValue_DoesNotThrow()
    {
        var (node, _) = MakeNode<string>();

        Assert.That(() => node.RemoveDirect("missing"), Throws.Nothing);
    }

    // --- Prune ---

    [Test]
    public void Prune_AfterSubdivideAndAllValuesRemoved_CollapsesToLeaf()
    {
        var (node, _) = MakeNode<string>(nodeCapacity: 2);
        var lookup = new Dictionary<string, QuadTreeNode<string>>();

        node.Insert(U2(-20, -20), "a", lookup);
        node.Insert(U2(20, -20), "b", lookup);
        node.Insert(U2(-20, 20), "c", lookup); // triggers subdivide

        Assert.That(node.IsLeaf, Is.False);

        lookup["a"].RemoveDirect("a");
        lookup["b"].RemoveDirect("b");
        lookup["c"].RemoveDirect("c");

        node.Prune();

        Assert.That(node.IsLeaf, Is.True);
    }

    [Test]
    public void Prune_WithRemainingValues_DoesNotCollapse()
    {
        var (node, _) = MakeNode<string>(nodeCapacity: 2);
        var lookup = new Dictionary<string, QuadTreeNode<string>>();

        node.Insert(U2(-20, -20), "a", lookup);
        node.Insert(U2(20, -20), "b", lookup);
        node.Insert(U2(-20, 20), "c", lookup); // triggers subdivide

        node.Prune(); // children still have values

        Assert.That(node.IsLeaf, Is.False);
    }

    // --- Clear ---

    [Test]
    public void Clear_AfterInserts_ResetsToLeafAndEmpty()
    {
        var (node, _) = MakeNode<string>(nodeCapacity: 2);
        var lookup = new Dictionary<string, QuadTreeNode<string>>();

        node.Insert(U2(-20, -20), "a", lookup);
        node.Insert(U2(20, -20), "b", lookup);
        node.Insert(U2(-20, 20), "c", lookup); // triggers subdivide

        node.Clear();

        Assert.Multiple(() =>
        {
            Assert.That(node.IsLeaf, Is.True);
            Assert.That(node.IsEmpty, Is.True);
        });
    }

    [Test]
    public void Clear_QueryReturnsNothing()
    {
        var (node, _) = MakeNode<string>();
        var lookup = new Dictionary<string, QuadTreeNode<string>>();

        node.Insert(U2(10, 10), "a", lookup);
        node.Clear();

        var results = new List<string>();
        node.Query(Bounds(0, 0, 100, 100), results.Add);

        Assert.That(results, Is.Empty);
    }

    // --- Query ---

    [Test]
    public void Query_PartialBounds_FindsOnlyValuesInRegion()
    {
        var (node, _) = MakeNode<string>(nodeCapacity: 2);
        var lookup = new Dictionary<string, QuadTreeNode<string>>();

        node.Insert(U2(-20, -20), "sw", lookup);
        node.Insert(U2(20, -20), "se", lookup);
        node.Insert(U2(-20, 20), "nw", lookup); // triggers subdivide

        var results = new List<string>();
        node.Query(Bounds(-25, 0, 50, 100), results.Add); // left half: [-50,-50] to [0,50]

        Assert.Multiple(() =>
        {
            Assert.That(results, Does.Contain("sw").And.Contain("nw"));
            Assert.That(results, Does.Not.Contain("se"));
        });
    }
}
