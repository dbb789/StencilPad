using System.Runtime.InteropServices;

namespace StencilPad.Spatial;

// It may be possible to replace this with a C#15 union in the future as long as
// any boxing concerns are addressed.
public struct PolygonSegment
{
    private enum Type : byte
    {
        Line,
        Arc,
        Bezier
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct Value
    {
        [FieldOffset(0)]
        public Line Line;

        [FieldOffset(0)]
        public Arc Arc;

        [FieldOffset(0)]
        public Bezier2D Bezier;
    }

    public static PolygonSegment FromLine(Line line)
    {
        return new PolygonSegment(Type.Line, new Value { Line = line });
    }

    public static PolygonSegment FromArc(Arc arc)
    {
        return new PolygonSegment(Type.Arc, new Value { Arc = arc });
    }

    public static PolygonSegment FromBezier(Bezier2D bezier)
    {
        return new PolygonSegment(Type.Bezier, new Value { Bezier = bezier });
    }

    private readonly Type _type;
    private readonly Value _value;

    public bool IsLine => _type == Type.Line;
    public bool IsArc => _type == Type.Arc;
    public bool IsBezier => _type == Type.Bezier;

    public Line Line => _type == Type.Line ? _value.Line :
        throw new InvalidOperationException("PolygonSegment does not contain a Line.");

    public Arc Arc => _type == Type.Arc ? _value.Arc :
        throw new InvalidOperationException("PolygonSegment does not contain an Arc.");

    public Bezier2D Bezier => _type == Type.Bezier ? _value.Bezier :
        throw new InvalidOperationException("PolygonSegment does not contain a Bezier.");

    public PolygonSegment Subsegment(double start, double end)
    {
        if (start <= 0.0 && end >= 1.0)
        {
            return this;
        }

        return _type switch
        {
            Type.Line   => FromLine(_value.Line.Subsegment(start, end)),
            Type.Arc    => FromArc(_value.Arc.Subsegment(start, end)),
            Type.Bezier => FromBezier(_value.Bezier.Subsegment(start, end)),
            _           => throw new InvalidOperationException("Unknown polygon segment type.")
        };
    }

    private PolygonSegment(Type type, Value value)
    {
        _type = type;
        _value = value;
    }
}
