namespace StencilPad.Tests.Spatial;

using StencilPad.Spatial;

public class PolygonTests
{
    private static Unit U(double v) => Unit.FromMillimeters(v);
    private static Unit2D U2(double x, double y) => new(U(x), U(y));

    [Test]
    public void MirrorX_PreservesSymmetry()
    {
        var polygon = new Polygon();
        polygon.AddVertex(new Vertex(U2(0, 0)));
        polygon.AddVertex(new Vertex(U2(10, 0)));
        polygon.AddVertex(new Vertex(U2(10, 10)));
        polygon.Close();

        // Set a symmetric control point
        // Edge 0 (V0->V1) ControlEndOffset
        // Edge 1 (V1->V2) ControlBeginOffset
        polygon.Edges[0] = polygon.Edges[0] with { ControlEndOffset = U2(2, 1) };
        
        // EdgeReassigned should have set Edge 1's ControlBeginOffset to (-2, -1)
        Assert.That(polygon.Edges[1].ControlBeginOffset, Is.EqualTo(U2(-2, -1)));

        // Mirror across Y=5
        polygon.MirrorX(U(5));

        // Expected V0: (0, 10), V1: (10, 10), V2: (10, 0)
        Assert.Multiple(() =>
        {
            Assert.That(polygon.Vertices[0].Position, Is.EqualTo(U2(0, 10)));
            Assert.That(polygon.Vertices[1].Position, Is.EqualTo(U2(10, 10)));
            Assert.That(polygon.Vertices[2].Position, Is.EqualTo(U2(10, 0)));

            // Expected offsets: Y negated
            // Edge 0 ControlEnd: (2, -1)
            // Edge 1 ControlBegin: (-2, 1)
            Assert.That(polygon.Edges[0].ControlEndOffset, Is.EqualTo(U2(2, -1)), "Edge 0 ControlEndOffset");
            Assert.That(polygon.Edges[1].ControlBeginOffset, Is.EqualTo(U2(-2, 1)), "Edge 1 ControlBeginOffset");
        });
    }
}
