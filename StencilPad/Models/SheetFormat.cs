using StencilPad.Spatial;

namespace StencilPad.Models;

public record SheetFormat
{
    public static Unit2D MaxSize => Unit2D.FromMillimeters(1200, 1200);
    public static Unit2D MinSize => Unit2D.FromMillimeters(100, 100);

    public SheetSizeType SizeType { get; init; }
    public SheetOrientation Orientation { get; init; }
    public Unit2D CustomSize { get; init; }

    public Unit2D Size
    {
        get
        {
            if (SizeType == SheetSizeType.Custom)
            {
                return CustomSize;
            }

            return GetSize(SizeType, Orientation);
        }
    }

    public SheetFormat()
        : this(SheetSizeType.A4, SheetOrientation.Portrait)
    { }

    public SheetFormat(SheetSizeType sizeType,
                       SheetOrientation orientation)
    {
        SizeType = sizeType;
        Orientation = orientation;
        CustomSize = GetSize(sizeType, orientation);
    }

    public SheetFormat(Unit2D customSize)
    {
        SizeType = SheetSizeType.Custom;
        Orientation = SheetOrientation.Portrait;
        CustomSize = new Unit2D(Unit.Clamp(MinSize.X, customSize.X, MaxSize.X),
                                Unit.Clamp(MinSize.Y, customSize.Y, MaxSize.Y));
    }
    
    private static Unit2D GetSize(SheetSizeType sizeType, SheetOrientation orientation)
    {
        var size = sizeType switch
        {
            SheetSizeType.A5 => Unit2D.FromMillimeters(148, 210),
            SheetSizeType.A4 => Unit2D.FromMillimeters(210, 297),
            SheetSizeType.A3 => Unit2D.FromMillimeters(297, 420),
            SheetSizeType.A2 => Unit2D.FromMillimeters(420, 594),
            SheetSizeType.A1 => Unit2D.FromMillimeters(594, 841),
            SheetSizeType.A0 => Unit2D.FromMillimeters(841, 1189),
            SheetSizeType.Letter => Unit2D.FromInches(8.5, 11),
            SheetSizeType.Legal => Unit2D.FromInches(8.5, 14),
            _ => Unit2D.FromMillimeters(210, 297)
        };

        if (orientation == SheetOrientation.Landscape)
        {
            return new Unit2D(size.Y, size.X);
        }

        return size;
    }
}
