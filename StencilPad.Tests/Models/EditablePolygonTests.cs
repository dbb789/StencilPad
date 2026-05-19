namespace StencilPad.Tests.Models;

using StencilPad.Spatial;
using StencilPad.Models;

[TestFixture]
public class EditablePolygonTests
{
    private static Unit U(double v) => Unit.FromMillimeters(v);
    private static Unit2D U2(double x, double y) => new(U(x), U(y));

    [Test]
    public void HandleLifecycle_AddVertex_CreatesHandle()
    {
        var polygon = new EditablePolygon();
        Handle? addedHandle = null;
        polygon.HandleAdded += (s, h, p, sel) => addedHandle = h;

        polygon.AddVertex(new Vertex(U2(10, 20)));

        Assert.Multiple(() =>
        {
            Assert.That(addedHandle, Is.Not.Null);
            var key = addedHandle!.Value.GetKey<PolygonHandleKey>();
            Assert.That(key.Type, Is.EqualTo(PolygonHandleType.Vertex));
            Assert.That(polygon.GetPoint(addedHandle.Value), Is.EqualTo(U2(10, 20)));
        });
    }

    [Test]
    public void HandleLifecycle_RemoveVertex_RemovesHandle()
    {
        var polygon = new EditablePolygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));
        
        Handle? removedHandle = null;
        polygon.HandleRemoved += (s, h) => removedHandle = h;

        polygon.DeleteVertex(0);

        Assert.That(removedHandle, Is.Not.Null);
    }

    [Test]
    public void HandleLifecycle_EdgeTypeChange_UpdatesHandles()
    {
        var polygon = new EditablePolygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));
        
        int addedCount = 0;
        polygon.HandleAdded += (s, h, p, sel) => {
            if (h.GetKey<PolygonHandleKey>().Type != PolygonHandleType.Vertex) addedCount++;
        };

        // Change Edge 0 to Bezier
        polygon.Edges[0] = polygon.Edges[0] with { Type = EdgeType.Bezier };

        Assert.That(addedCount, Is.EqualTo(2), "Should add two control handles");
    }

    [Test]
    public void Movement_VertexMove_MovesControlPoints()
    {
        var polygon = new EditablePolygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));
        polygon.Edges[0] = new Edge(U2(1, 1), U2(2, 2)) { Type = EdgeType.Bezier };

        var movedHandles = new List<Handle>();
        polygon.HandleMoved += (s, h, p) => movedHandles.Add(h);

        // Move Vertex 1
        polygon.SetPoint(polygon.GetVertexHandle(polygon.Vertices.KeyAt(1)), U2(20, 0));

        // Vertex 1 moved -> Should move Vertex 1 handle AND Edge 0 ControlEnd handle
        Assert.Multiple(() =>
        {
            Assert.That(movedHandles.Any(h => h.GetKey<PolygonHandleKey>().Type == PolygonHandleType.Vertex), Is.True);
            Assert.That(movedHandles.Any(h => h.GetKey<PolygonHandleKey>().Type == PolygonHandleType.ControlEnd), Is.True);
        });
    }

    [Test]
    public void Movement_OpenPolygon_VertexZero_DoesNotWrap()
    {
        var polygon = new EditablePolygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));
        polygon.AddVertex(new Vertex(U2(20, 0)));
        
        // Edge 1 is Bezier
        polygon.Edges[1] = polygon.Edges[1] with { Type = EdgeType.Bezier, ControlBeginOffset = U2(1,1) };

        var movedHandles = new List<Handle>();
        polygon.HandleMoved += (s, h, p) => movedHandles.Add(h);

        // Move Vertex 0. Should NOT move Edge 1 handles in an open polygon.
        polygon.Vertices[0] = polygon.Vertices[0] with { Position = U2(-10, 0) };

        Assert.That(movedHandles.Count, Is.EqualTo(1), "Only Vertex 0 handle should move");
        Assert.That(movedHandles[0].GetKey<PolygonHandleKey>().Type, Is.EqualTo(PolygonHandleType.Vertex));
    }

    [Test]
    public void Selection_LazyReindexing_ClearsDirtyFlag()
    {
        var polygon = new EditablePolygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));
        
        var v0Handle = polygon.GetVertexHandle(polygon.Vertices.KeyAt(0));
        polygon.SetHandleSelected(v0Handle, true);

        // First call triggers update
        var selected = polygon.GetSelectedVertices();
        Assert.That(selected.Count, Is.EqualTo(1));

        // Modify geometry - should mark dirty
        polygon.AddVertex(new Vertex(U2(20, 0)));
        
        // Verifying it is re-evaluated (index of selection might change if we inserted, but here we appended)
        Assert.That(polygon.GetSelectedVertices().Count, Is.EqualTo(1));
    }

    [Test]
    public void Selection_EdgeSelection_DetectedBetweenVertices()
    {
        var polygon = new EditablePolygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));
        polygon.AddVertex(new Vertex(U2(20, 0)));

        polygon.SetHandleSelected(polygon.GetVertexHandle(polygon.Vertices.KeyAt(0)), true);
        polygon.SetHandleSelected(polygon.GetVertexHandle(polygon.Vertices.KeyAt(1)), true);

        Assert.Multiple(() =>
        {
            Assert.That(polygon.GetSelectedVertices(), Is.EquivalentTo(new[] { 0, 1 }));
            Assert.That(polygon.GetSelectedEdges(), Is.EquivalentTo(new[] { 0 }));
        });
    }

    [Test]
    public void AssignFrom_EditableToEditable_PreservesStateAndId()
    {
        var source = new EditablePolygon();
        source.AddVertex(new Vertex(U2(0, 0)));
        var v0Handle = source.GetVertexHandle(source.Vertices.KeyAt(0));
        source.SetHandleSelected(v0Handle, true);

        var target = new EditablePolygon();
        target.AssignFrom(source);

        Assert.Multiple(() =>
        {
            Assert.That(target.Vertices.Count, Is.EqualTo(1));
            Assert.That(target.GetSelectedVertices(), Is.EquivalentTo(new[] { 0 }));
            // Verification of shared ID as per design requirement
            Assert.That(target.GetPoint(v0Handle), Is.EqualTo(U2(0, 0)));
        });
    }
}

internal static class HandleSourceExtensions
{
    public static Handle GetVertexHandle(this IHandleSource source, ulong key)
    {
        Handle? result = null;
        source.QueryHandles((h, p, s) => {
            var pk = h.GetKey<PolygonHandleKey>();
            if (pk.Type == PolygonHandleType.Vertex && pk.Key == key) result = h;
        });
        return result ?? throw new KeyNotFoundException();
    }
}
