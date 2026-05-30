namespace StencilPad.Spatial;

public interface IGeometryWalker
{
    bool Begin(int segmentCount, bool closed);
    bool Segment(int segmentIndex, PolygonSegment segment);
}
