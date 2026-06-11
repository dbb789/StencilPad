using StencilPad.Models;

namespace StencilPad.Schemas;

public enum SheetSizeTypeSchema
{
    A5,
    A4,
    A3,
    A2,
    A1,
    A0,
    Letter,
    Legal,
    Custom
}

public static class SheetSizeTypeSchemaUtil
{
    public static SheetSizeTypeSchema Pack(SheetSizeType sizeType)
    {
        return sizeType switch
        {
            SheetSizeType.A5 => SheetSizeTypeSchema.A5,
            SheetSizeType.A4 => SheetSizeTypeSchema.A4,
            SheetSizeType.A3 => SheetSizeTypeSchema.A3,
            SheetSizeType.A2 => SheetSizeTypeSchema.A2,
            SheetSizeType.A1 => SheetSizeTypeSchema.A1,
            SheetSizeType.A0 => SheetSizeTypeSchema.A0,
            SheetSizeType.Letter => SheetSizeTypeSchema.Letter,
            SheetSizeType.Legal => SheetSizeTypeSchema.Legal,
            SheetSizeType.Custom => SheetSizeTypeSchema.Custom,
            _ => throw new ArgumentOutOfRangeException(nameof(sizeType), $"Unsupported sheet size type: {sizeType}")
        };
    }

    public static SheetSizeType Unpack(SheetSizeTypeSchema data)
    {
        return data switch
        {
            SheetSizeTypeSchema.A5 => SheetSizeType.A5,
            SheetSizeTypeSchema.A4 => SheetSizeType.A4,
            SheetSizeTypeSchema.A3 => SheetSizeType.A3,
            SheetSizeTypeSchema.A2 => SheetSizeType.A2,
            SheetSizeTypeSchema.A1 => SheetSizeType.A1,
            SheetSizeTypeSchema.A0 => SheetSizeType.A0,
            SheetSizeTypeSchema.Letter => SheetSizeType.Letter,
            SheetSizeTypeSchema.Legal => SheetSizeType.Legal,
            SheetSizeTypeSchema.Custom => SheetSizeType.Custom,
            _ => throw new ArgumentOutOfRangeException(nameof(data), $"Unsupported sheet size type schema: {data}")
        };
    }
}
