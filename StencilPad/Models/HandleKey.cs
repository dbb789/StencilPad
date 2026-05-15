using System.Runtime.InteropServices;

namespace StencilPad.Models;

public readonly struct HandleKey : IEquatable<HandleKey>, IComparable<HandleKey>
{
    public static readonly HandleKey None = new HandleKey(Type.None, new Value());
    
    private enum Type : byte
    {
        None,
        Polygon,
        StartEnd
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct Value
    {
        [FieldOffset(0)]
        public PolygonHandleKey PolygonKey;

        [FieldOffset(0)]
        public StartEndHandleKey StartEndKey;
    }

    private readonly Type _type;
    private readonly Value _value;

    public HandleKey(PolygonHandleKey polygon)
    {
        _type = Type.Polygon;
        _value = new Value { PolygonKey = polygon };
    }

    public HandleKey(StartEndHandleKey startEnd)
    {
        _type = Type.StartEnd;
        _value = new Value { StartEndKey = startEnd };
    }

    public PolygonHandleKey Polygon
    {
        get
        {
            if (_type != Type.Polygon)
            {
                throw new InvalidOperationException("HandleKey does not contain a PolygonHandleKey.");
            }
            
            return _value.PolygonKey;
        }
    }
    
    public StartEndHandleKey StartEnd
    {
        get
        {
            if (_type != Type.StartEnd)
            {
                throw new InvalidOperationException("HandleKey does not contain a StartEndHandleKey.");
            }
            
            return _value.StartEndKey;
        }
    }

    private HandleKey(Type type, Value value)
    {
        _type = type;
        _value = value;
    }

    public bool Equals(HandleKey other)
    {
        if (_type != other._type)
        {
            return false;
        }

        return _type switch
        {
            Type.None => true,
            Type.Polygon => Polygon.Equals(other.Polygon),
            Type.StartEnd => StartEnd.Equals(other.StartEnd),
            _ => throw new InvalidOperationException("Invalid HandleKey type.")
        };
    }

    public override bool Equals(object? obj)
    {
        return obj is HandleKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _type switch
        {
            Type.None => 0,
            Type.Polygon => Polygon.GetHashCode(),
            Type.StartEnd => StartEnd.GetHashCode(),
            _ => throw new InvalidOperationException("Invalid HandleKey type.")
        };
    }

    public int CompareTo(HandleKey other)
    {
        int cmp = _type.CompareTo(other._type);
        
        if (cmp != 0)
        {
            return cmp;
        }

        return _type switch
        {
            Type.None => 0,
            Type.Polygon => Polygon.CompareTo(other.Polygon),
            Type.StartEnd => StartEnd.CompareTo(other.StartEnd),
            _ => throw new InvalidOperationException("Invalid HandleKey type.")
        };
    }
    
    public static bool operator==(HandleKey lhs, HandleKey rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator!=(HandleKey lhs, HandleKey rhs)
    {
        return !(lhs == rhs);
    }
}

