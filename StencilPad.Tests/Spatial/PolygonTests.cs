namespace StencilPad.Tests.Spatial;

using StencilPad.Spatial;

[TestFixture]
public class PolygonTests
{
    private static Unit U(double v) => Unit.FromMillimeters(v);
    private static Unit2D U2(double x, double y) => new(U(x), U(y));

    [Test]
    public void Lifecycle_AddVertex_AppendsVertices()
    {
        var polygon = new Polygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));

        Assert.Multiple(() =>
        {
            Assert.That(polygon.Vertices.Count, Is.EqualTo(2));
            Assert.That(polygon.Vertices[0].Position, Is.EqualTo(U2(0, 0)));
            Assert.That(polygon.Vertices[1].Position, Is.EqualTo(U2(10, 0)));
            Assert.That(polygon.Edges.Count, Is.EqualTo(1));
            Assert.That(polygon.Closed, Is.False);
        });
    }

    [Test]
    public void Lifecycle_InsertVertex_MaintainsOrderAndEdges()
    {
        var polygon = new Polygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(20, 0)));
        
        polygon.InsertVertex(1, new Vertex(U2(10, 0)));

        Assert.Multiple(() =>
        {
            Assert.That(polygon.Vertices.Count, Is.EqualTo(3));
            Assert.That(polygon.Vertices[0].Position, Is.EqualTo(U2(0, 0)));
            Assert.That(polygon.Vertices[1].Position, Is.EqualTo(U2(10, 0)));
            Assert.That(polygon.Vertices[2].Position, Is.EqualTo(U2(20, 0)));
            Assert.That(polygon.Edges.Count, Is.EqualTo(2));
        });
    }

    [Test]
    public void Lifecycle_DeleteVertex_OpenPolygon()
    {
        var polygon = new Polygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));
        polygon.AddVertex(new Vertex(U2(20, 0)));

        polygon.DeleteVertex(1);

        Assert.Multiple(() =>
        {
            Assert.That(polygon.Vertices.Count, Is.EqualTo(2));
            Assert.That(polygon.Vertices[0].Position, Is.EqualTo(U2(0, 0)));
            Assert.That(polygon.Vertices[1].Position, Is.EqualTo(U2(20, 0)));
            Assert.That(polygon.Edges.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void Lifecycle_DeleteVertex_ClosedPolygon()
    {
        var polygon = new Polygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));
        polygon.AddVertex(new Vertex(U2(10, 10)));
        polygon.AddVertex(new Vertex(U2(0, 10)));
        polygon.Close();

        polygon.DeleteVertex(1);

        Assert.Multiple(() =>
        {
            Assert.That(polygon.Vertices.Count, Is.EqualTo(3));
            Assert.That(polygon.Edges.Count, Is.EqualTo(3));
            Assert.That(polygon.Closed, Is.True);
        });
    }

    [Test]
    public void Lifecycle_Clear_ResetsPolygon()
    {
        var polygon = new Polygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));
        polygon.Close();

        polygon.Clear();

        Assert.Multiple(() =>
        {
            Assert.That(polygon.Vertices.Count, Is.EqualTo(0));
            Assert.That(polygon.Edges.Count, Is.EqualTo(0));
            Assert.That(polygon.Closed, Is.False);
        });
    }

    [Test]
    public void Transition_Close_RequiresMinThreeVertices()
    {
        var polygon = new Polygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));
        
        polygon.Close();
        Assert.That(polygon.Closed, Is.False);

        polygon.AddVertex(new Vertex(U2(10, 10)));
        polygon.Close();
        Assert.Multiple(() =>
        {
            Assert.That(polygon.Closed, Is.True);
            Assert.That(polygon.Edges.Count, Is.EqualTo(3));
        });
    }

    [Test]
    public void Transition_Open_RemovesCorrectEdgeAndRotates()
    {
        var polygon = new Polygon();
        polygon.AddVertex(new Vertex(U2(0, 0))); // V0
        polygon.AddVertex(new Vertex(U2(10, 0))); // V1
        polygon.AddVertex(new Vertex(U2(10, 10))); // V2
        polygon.Close(); // E0: V0-V1, E1: V1-V2, E2: V2-V0

        // Open(0) breaks edge E0 (V0-V1).
        // This causes V1 to become the new first vertex (index 0).
        // The new order should be V1, V2, V0.
        polygon.Open(0);

        Assert.Multiple(() =>
        {
            Assert.That(polygon.Closed, Is.False);
            Assert.That(polygon.Vertices.Count, Is.EqualTo(3));
            Assert.That(polygon.Vertices[0].Position, Is.EqualTo(U2(10, 0)));
            Assert.That(polygon.Vertices[1].Position, Is.EqualTo(U2(10, 10)));
            Assert.That(polygon.Vertices[2].Position, Is.EqualTo(U2(0, 0)));
            Assert.That(polygon.Edges.Count, Is.EqualTo(2));
        });
    }

    [Test]
    public void Geometric_Translate_MovesAllVertices()
    {
        var polygon = new Polygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));
        
        polygon.Translate(U2(5, 5));

        Assert.Multiple(() =>
        {
            Assert.That(polygon.Vertices[0].Position, Is.EqualTo(U2(5, 5)));
            Assert.That(polygon.Vertices[1].Position, Is.EqualTo(U2(15, 5)));
        });
    }

    [Test]
    public void Geometric_MirrorX_MirrorsVerticesAndEdges()
    {
        var polygon = new Polygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 5)));
        polygon.Edges[0] = new Edge { ControlBeginOffset = U2(1, 2), ControlEndOffset = U2(3, 4) };

        polygon.MirrorX(U(0)); // Mirror across Y=0

        Assert.Multiple(() =>
        {
            Assert.That(polygon.Vertices[0].Position, Is.EqualTo(U2(0, 0)));
            Assert.That(polygon.Vertices[1].Position, Is.EqualTo(U2(10, -5)));
            Assert.That(polygon.Edges[0].ControlBeginOffset, Is.EqualTo(U2(1, -2)));
            Assert.That(polygon.Edges[0].ControlEndOffset, Is.EqualTo(U2(3, -4)));
        });
    }

    [Test]
    public void Geometric_MirrorY_MirrorsVerticesAndEdges()
    {
        var polygon = new Polygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(5, 10)));
        polygon.Edges[0] = new Edge { ControlBeginOffset = U2(1, 2), ControlEndOffset = U2(3, 4) };

        polygon.MirrorY(U(0)); // Mirror across X=0

        Assert.Multiple(() =>
        {
            Assert.That(polygon.Vertices[0].Position, Is.EqualTo(U2(0, 0)));
            Assert.That(polygon.Vertices[1].Position, Is.EqualTo(U2(-5, 10)));
            Assert.That(polygon.Edges[0].ControlBeginOffset, Is.EqualTo(U2(-1, 2)));
            Assert.That(polygon.Edges[0].ControlEndOffset, Is.EqualTo(U2(-3, 4)));
        });
    }

    [Test]
    public void Symmetry_EdgeReassigned_UpdatesNeighbors()
    {
        var polygon = new Polygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));
        polygon.AddVertex(new Vertex(U2(10, 10)));
        polygon.Close();

        // Edge 0 (V0-V1), Edge 1 (V1-V2), Edge 2 (V2-V0)
        // Modify Edge 0 ControlEndOffset -> should update Edge 1 ControlBeginOffset
        polygon.Edges[0] = polygon.Edges[0] with { ControlEndOffset = U2(2, 3) };
        Assert.That(polygon.Edges[1].ControlBeginOffset, Is.EqualTo(U2(-2, -3)));

        // Modify Edge 1 ControlBeginOffset -> should update Edge 0 ControlEndOffset
        polygon.Edges[1] = polygon.Edges[1] with { ControlBeginOffset = U2(-4, -5) };
        Assert.That(polygon.Edges[0].ControlEndOffset, Is.EqualTo(U2(4, 5)));
        
        // Wrap around: Modify Edge 2 ControlEndOffset -> should update Edge 0 ControlBeginOffset
        polygon.Edges[2] = polygon.Edges[2] with { ControlEndOffset = U2(1, 1) };
        Assert.That(polygon.Edges[0].ControlBeginOffset, Is.EqualTo(U2(-1, -1)));
    }

    [Test]
    public void AutoOpen_DeleteVertex_WhenCountDropsBelowThree()
    {
        var polygon = new Polygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));
        polygon.AddVertex(new Vertex(U2(10, 10)));
        polygon.Close();
        
        Assert.That(polygon.Closed, Is.True);
        
        polygon.DeleteVertex(2);
        
        Assert.Multiple(() =>
        {
            Assert.That(polygon.Closed, Is.False);
            Assert.That(polygon.Vertices.Count, Is.EqualTo(2));
            Assert.That(polygon.Edges.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void Events_FiresWhenExpected()
    {
        var polygon = new Polygon();
        int vertexAddedCount = 0;
        int vertexRemovedCount = 0;
        int edgeAddedCount = 0;
        int edgeRemovedCount = 0;
        int geometryChangedCount = 0;

        polygon.VertexAdded += (i, k) => vertexAddedCount++;
        polygon.VertexRemoved += (i, k) => vertexRemovedCount++;
        polygon.EdgeAdded += (i, k) => edgeAddedCount++;
        polygon.EdgeRemoved += (i, k) => edgeRemovedCount++;
        polygon.GeometryChanged += () => geometryChangedCount++;

        // Add first vertex
        polygon.AddVertex(new Vertex(U2(0, 0)));
        // VertexAdded: 1, EdgeAdded: 0, GeometryChanged: 1

        // Add second vertex
        polygon.AddVertex(new Vertex(U2(10, 0)));
        // VertexAdded: 2, EdgeAdded: 1, GeometryChanged: 2

        // Close
        polygon.AddVertex(new Vertex(U2(10, 10)));
        // VertexAdded: 3, EdgeAdded: 2, GeometryChanged: 3
        polygon.Close();
        // EdgeAdded: 3, ClosedChanged: 1, GeometryChanged: 4

        // Delete vertex
        polygon.DeleteVertex(1);
        // VertexRemoved: 1, EdgeRemoved: 1, GeometryChanged: 5

        // Clear
        polygon.Clear();
        // VertexRemoved: 1+2=3, EdgeRemoved: 1+2=3, GeometryChanged: 6

        Assert.Multiple(() =>
        {
            Assert.That(vertexAddedCount, Is.EqualTo(3), "VertexAdded");
            Assert.That(vertexRemovedCount, Is.EqualTo(3), "VertexRemoved");
            Assert.That(edgeAddedCount, Is.EqualTo(3), "EdgeAdded");
            Assert.That(edgeRemovedCount, Is.EqualTo(3), "EdgeRemoved");
            Assert.That(geometryChangedCount, Is.GreaterThanOrEqualTo(6), "GeometryChanged");
        });
    }

    [Test]
    public void Cloning_DeepClone_CreatesIndependentCopy()
    {
        var polygon = new Polygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));
        polygon.AddVertex(new Vertex(U2(10, 10)));
        polygon.Close();
        polygon.Edges[0] = polygon.Edges[0] with { ControlEndOffset = U2(1, 1) };

        var clone = polygon.DeepClone();

        Assert.Multiple(() =>
        {
            Assert.That(clone.Vertices.Count, Is.EqualTo(polygon.Vertices.Count));
            Assert.That(clone.Edges.Count, Is.EqualTo(polygon.Edges.Count));
            Assert.That(clone.Closed, Is.EqualTo(polygon.Closed));
            Assert.That(clone.Edges[0].ControlEndOffset, Is.EqualTo(polygon.Edges[0].ControlEndOffset));
        });

        // Modify clone, original should not change
        clone.Translate(U2(100, 100));
        Assert.That(polygon.Vertices[0].Position, Is.EqualTo(U2(0, 0)));

        // Modify edge in clone
        clone.Edges[0] = clone.Edges[0] with { ControlEndOffset = U2(5, 5) };
        Assert.That(polygon.Edges[0].ControlEndOffset, Is.EqualTo(U2(1, 1)));
    }
}
