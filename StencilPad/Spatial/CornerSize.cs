using System.Runtime.InteropServices;

namespace StencilPad.Spatial;

// It may be possible to replace this with a C#15 union in the future as long as
// any boxing concerns are addressed.
public struct CornerSize
{
    public enum Type : byte
    {
        Unit,
        Proportion
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct Value
    {
        [FieldOffset(0)]
        public Unit Unit;
        
        [FieldOffset(0)]
        public double Proportion;
    }

    public static CornerSize Zero = FromUnit(Unit.Zero);

    public static CornerSize FromUnit(Unit unit)
    {
        return new CornerSize(Type.Unit, new Value { Unit = unit });
    }

    public static CornerSize FromProportion(double proportion)
    {
        return new CornerSize(Type.Proportion, new Value { Proportion = proportion });
    }

    private readonly Type _type;
    private readonly Value _value;

    public bool IsUnit => _type == Type.Unit;
    public bool IsProportion => _type == Type.Proportion;

    public Unit Unit => _type == Type.Unit ? _value.Unit :
        throw new InvalidOperationException("CornerSize does not contain a Unit value.");
    
    public double Proportion => _type == Type.Proportion ? _value.Proportion :
        throw new InvalidOperationException("CornerSize does not contain a Proportion value.");
    
    private CornerSize(Type type, Value value)
    {
        _type = type;
        _value = value;
    }
}

