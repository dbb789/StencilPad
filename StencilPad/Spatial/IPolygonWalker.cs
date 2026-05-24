namespace StencilPad.Spatial;

public interface IPolygonWalker
{
    void Begin(Unit2D startPoint, bool closed);
    void Line(Unit2D from, Unit2D to);
    void Arc(Unit2D start, Unit2D mid, Unit2D end);
    void Bezier(Unit2D from, Unit2D c1, Unit2D c2, Unit2D to);
}
