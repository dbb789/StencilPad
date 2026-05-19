namespace StencilPad.Tests.Models;

using StencilPad.Spatial;
using StencilPad.Models;

[TestFixture]
public class EditablePolygonListTests
{
    private static Unit U(double v) => Unit.FromMillimeters(v);
    private static Unit2D U2(double x, double y) => new(U(x), U(y));

    [Test]
    public void Add_SyncsHandleSource()
    {
        var list = new EditablePolygonList();
        var poly = new EditablePolygon();
        poly.AddVertex(new Vertex(U2(10, 10)));

        int handleAddedCount = 0;
        list.HandleSource.HandleAdded += (s, h, p, sel) => handleAddedCount++;

        list.Add(poly);

        Assert.That(handleAddedCount, Is.EqualTo(1));
    }

    [Test]
    public void Remove_SyncsHandleSource()
    {
        var list = new EditablePolygonList();
        var poly = new EditablePolygon();
        poly.AddVertex(new Vertex(U2(10, 10)));
        list.Add(poly);

        int handleRemovedCount = 0;
        list.HandleSource.HandleRemoved += (s, h) => handleRemovedCount++;

        list.Remove(poly);

        Assert.That(handleRemovedCount, Is.EqualTo(1));
    }

    [Test]
    public void Position_PropagatesToHandleSource()
    {
        var list = new EditablePolygonList();
        var poly = new EditablePolygon();
        poly.AddVertex(new Vertex(U2(10, 10)));
        list.Add(poly);

        // Position 0,0 (default) -> Handle world pos is 10,10
        // Move list to 5,5 -> Handle world pos should be 15,15
        list.Position = U2(5, 5);

        var handle = list.HandleSource.GetAnyHandle();
        Assert.That(list.HandleSource.GetPoint(handle), Is.EqualTo(U2(15, 15)));
    }

    [Test]
    public void AssignFrom_SyncsStateAndPosition()
    {
        var source = new EditablePolygonList();
        var poly = new EditablePolygon();
        poly.AddVertex(new Vertex(U2(10, 10)));
        source.Add(poly);
        source.Position = U2(5, 5);

        var target = new EditablePolygonList();
        target.AssignFrom(source);

        Assert.Multiple(() =>
        {
            Assert.That(target.Count, Is.EqualTo(1));
            Assert.That(target.Position, Is.EqualTo(U2(5, 5)));
            var handle = target.HandleSource.GetAnyHandle();
            Assert.That(target.HandleSource.GetPoint(handle), Is.EqualTo(U2(15, 15)));
        });
    }

    [Test]
    public void Clear_RemovesAllPolygonsAndHandles()
    {
        var list = new EditablePolygonList();
        list.Add(new EditablePolygon());
        list.Add(new EditablePolygon());

        int removedCount = 0;
        list.PolygonRemoved += (p) => removedCount++;

        list.Clear();

        Assert.Multiple(() =>
        {
            Assert.That(list.Count, Is.EqualTo(0));
            Assert.That(removedCount, Is.EqualTo(2));
        });
    }
}

internal static class ListTestExtensions
{
    public static Handle GetAnyHandle(this IHandleSource source)
    {
        Handle? result = null;
        source.QueryHandles((h, p, s) => result = h);
        return result ?? throw new KeyNotFoundException();
    }
}
