namespace StencilPad.Spatial;

public interface IGeometryWalker
{
    bool Begin(int segmentCount, bool closed);
    bool Line(int segmentIndex, Unit2D from, Unit2D to);
    bool Arc(int segmentIndex, Unit2D start, Unit2D mid, Unit2D end);
    bool Bezier(int segmentIndex, Unit2D from, Unit2D c1, Unit2D c2, Unit2D to);
}
