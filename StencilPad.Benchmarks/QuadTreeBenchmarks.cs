using BenchmarkDotNet.Attributes;
using StencilPad.Spatial;

namespace StencilPad.Benchmarks;

/// <summary>
/// A minimal stand-in for HandleMapEntry: a class wrapping a unique int ID,
/// using that ID for equality so QuadTree&lt;T&gt; dictionary keying works the same way.
/// </summary>
public sealed class Entry(int id)
{
    public int Id { get; } = id;

    public override bool Equals(object? obj) => obj is Entry e && e.Id == Id;
    public override int GetHashCode() => Id;
}

/// <summary>
/// End-to-end QuadTree&lt;Entry&gt; benchmarks covering the operations HandleMap exercises:
/// bulk insert, bulk remove, bulk move, and spatial query.
///
/// N is parameterised over realistic handle counts a canvas might hold.
/// NodeCapacity and MaxDepth match HandleMap's usage (64, 32).
/// </summary>
[MemoryDiagnoser]
public class QuadTreeBenchmarks
{
    private const int NodeCapacity = 64;
    private const int MaxDepth = 32;
    private static readonly UnitBounds TreeBounds =
        UnitBounds.FromCenterSize(
            new Unit2D(Unit.FromMillimeters(0), Unit.FromMillimeters(0)),
            new Unit2D(Unit.FromMillimeters(1000), Unit.FromMillimeters(1000)));

    [Params(64, 256, 1024)]
    public int N;

    private Entry[] _entries = null!;
    private Unit2D[] _initialPoints = null!;
    private Unit2D[] _movedPoints = null!;

    // For query benchmarks — a small region covering ~10% of the canvas.
    private UnitBounds _queryBounds;

    // For remove/move benchmarks — pre-filled trees that are rebuilt each iteration.
    private QuadTree<Entry> _tree = null!;
    private ObjectPool<QuadTreeNode<Entry>> _pool = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);

        _entries = Enumerable.Range(0, N).Select(i => new Entry(i)).ToArray();

        _initialPoints = _entries
            .Select(_ => RandomPoint(rng))
            .ToArray();

        _movedPoints = _entries
            .Select(_ => RandomPoint(rng))
            .ToArray();

        _queryBounds = UnitBounds.FromCenterSize(
            new Unit2D(Unit.FromMillimeters(0), Unit.FromMillimeters(0)),
            new Unit2D(Unit.FromMillimeters(100), Unit.FromMillimeters(100)));

        _pool = new ObjectPool<QuadTreeNode<Entry>>(N * 2);
    }

    [IterationSetup(Targets =
    [
        nameof(BulkRemove),
        nameof(BulkMove),
        nameof(Query),
        nameof(QueryAll)
    ])]
    public void FillTree()
    {
        _tree = new QuadTree<Entry>(_pool, TreeBounds, NodeCapacity, MaxDepth);

        for (int i = 0; i < N; i++)
        {
            _tree.Insert(_initialPoints[i], _entries[i]);
        }
    }

    [IterationCleanup(Targets =
    [
        nameof(BulkRemove),
        nameof(BulkMove),
        nameof(Query),
        nameof(QueryAll)
    ])]
    public void DisposeTree() => _tree.Dispose();

    // -------------------------------------------------------------------------
    // BulkInsert — inserts N entries into a fresh tree each iteration.
    // -------------------------------------------------------------------------

    [Benchmark]
    public QuadTree<Entry> BulkInsert()
    {
        var tree = new QuadTree<Entry>(_pool, TreeBounds, NodeCapacity, MaxDepth);

        for (int i = 0; i < N; i++)
        {
            tree.Insert(_initialPoints[i], _entries[i]);
        }

        tree.Dispose();

        return tree;
    }

    // -------------------------------------------------------------------------
    // BulkRemove — removes all N entries from a pre-filled tree.
    // -------------------------------------------------------------------------

    [Benchmark]
    public int BulkRemove()
    {
        int removed = 0;

        for (int i = 0; i < N; i++)
        {
            if (_tree.Remove(_entries[i]))
            {
                removed++;
            }
        }

        return removed;
    }

    // -------------------------------------------------------------------------
    // BulkMove — moves all N entries to new positions.
    // -------------------------------------------------------------------------

    [Benchmark]
    public int BulkMove()
    {
        int moved = 0;

        for (int i = 0; i < N; i++)
        {
            if (_tree.Move(_movedPoints[i], _entries[i]))
            {
                moved++;
            }
        }

        return moved;
    }

    // -------------------------------------------------------------------------
    // Query — spatial range query over ~10% of the canvas.
    // -------------------------------------------------------------------------

    [Benchmark]
    public int Query()
    {
        int count = 0;
        _tree.Query(_queryBounds, _ => count++);
        return count;
    }

    // -------------------------------------------------------------------------
    // QueryAll — visit every value (baseline for Query overhead).
    // -------------------------------------------------------------------------

    [Benchmark]
    public int QueryAll()
    {
        int count = 0;
        _tree.VisitAllValues((_, _) => count++);
        return count;
    }

    // -------------------------------------------------------------------------

    private static Unit2D RandomPoint(Random rng)
    {
        var x = (rng.NextDouble() - 0.5) * 1000;
        var y = (rng.NextDouble() - 0.5) * 1000;
        return new Unit2D(Unit.FromMillimeters(x), Unit.FromMillimeters(y));
    }
}
