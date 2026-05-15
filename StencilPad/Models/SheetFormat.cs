using StencilPad.Spatial;

namespace StencilPad.Models;

public record SheetFormat
{
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
        CustomSize = customSize;
    }
    
    private static Unit2D GetSize(SheetSizeType sizeType, SheetOrientation orientation)
    {
        var size = sizeType switch
        {
            SheetSizeType.A5 => new Unit2D(Unit.FromMillimeters(148), Unit.FromMillimeters(210)),
            SheetSizeType.A4 => new Unit2D(Unit.FromMillimeters(210), Unit.FromMillimeters(297)),
            SheetSizeType.A3 => new Unit2D(Unit.FromMillimeters(297), Unit.FromMillimeters(420)),
            SheetSizeType.A2 => new Unit2D(Unit.FromMillimeters(420), Unit.FromMillimeters(594)),
            SheetSizeType.A1 => new Unit2D(Unit.FromMillimeters(594), Unit.FromMillimeters(841)),
            SheetSizeType.A0 => new Unit2D(Unit.FromMillimeters(841), Unit.FromMillimeters(1189)),
            SheetSizeType.Letter => new Unit2D(Unit.FromInches(8.5), Unit.FromInches(11)),
            SheetSizeType.Legal => new Unit2D(Unit.FromInches(8.5), Unit.FromInches(14)),
            _ => new Unit2D(Unit.FromMillimeters(210), Unit.FromMillimeters(297))
        };

        if (orientation == SheetOrientation.Landscape)
        {
            return new Unit2D(size.Y, size.X);
        }

        return size;
    }
}
