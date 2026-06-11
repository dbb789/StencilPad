using StencilPad.Spatial;
using StencilPad.Models;

namespace StencilPad.Schemas;

public class SheetFormatSchema
{
    public SheetSizeTypeSchema SizeType { get; set; } = SheetSizeTypeSchema.A4;
    public SheetOrientationSchema Orientation { get; set; } = SheetOrientationSchema.Portrait;
    public Unit2D CustomSize { get; set; } = Unit2D.FromMillimeters(210, 297);

    public static SheetFormatSchema Pack(SheetFormat format)
    {
        return new SheetFormatSchema
        {
            SizeType = SheetSizeTypeSchemaUtil.Pack(format.SizeType),
            Orientation = SheetOrientationSchemaUtil.Pack(format.Orientation),
            CustomSize = format.CustomSize,
        };
    }

    public static SheetFormat Unpack(SheetFormatSchema data)
    {
        return new SheetFormat
        {
            SizeType = SheetSizeTypeSchemaUtil.Unpack(data.SizeType),
            Orientation = SheetOrientationSchemaUtil.Unpack(data.Orientation),
            CustomSize = data.CustomSize,
        };
    }
}
