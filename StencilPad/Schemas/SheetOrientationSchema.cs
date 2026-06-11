using StencilPad.Models;

namespace StencilPad.Schemas;

public enum SheetOrientationSchema
{
    Portrait = 0,
    Landscape = 1
}

public static class SheetOrientationSchemaUtil
{
    public static SheetOrientationSchema Pack(SheetOrientation orientation)
    {
        return orientation switch
        {
            SheetOrientation.Portrait => SheetOrientationSchema.Portrait,
            SheetOrientation.Landscape => SheetOrientationSchema.Landscape,
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), $"Unsupported sheet orientation: {orientation}")
        };
    }

    public static SheetOrientation Unpack(SheetOrientationSchema data)
    {
        return data switch
        {
            SheetOrientationSchema.Portrait => SheetOrientation.Portrait,
            SheetOrientationSchema.Landscape => SheetOrientation.Landscape,
            _ => throw new ArgumentOutOfRangeException(nameof(data), $"Unsupported sheet orientation schema value: {data}")
        };
    }
}

