namespace StencilPad.Spatial;

public interface IGeometryWalker
{
    bool Begin(int segmentCount, bool closed);
    bool Line(int segmentIndex, Line line);
    bool Arc(int segmentIndex, Arc arc);
    bool Bezier(int segmentIndex, Bezier2D bezier);
}
