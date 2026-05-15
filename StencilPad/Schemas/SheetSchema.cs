using StencilPad.Models;

namespace StencilPad.Schemas;

public class SheetSchema
{
    public string Name { get; set; } = string.Empty;
    public SheetFormat Format { get; set; } = new SheetFormat(SheetSizeType.A4,
                                                              SheetOrientation.Portrait);
    public SheetElementSchema[] Elements { get; set; } = [];

    public static SheetSchema Pack(Sheet sheet)
    {
        var elements = sheet.Elements
            .Select(SheetElementSchema.Pack)
            .Where(s => s is not null)
            .Cast<SheetElementSchema>()
            .ToArray();

        return new SheetSchema
        {
            Name = sheet.Name,
            Format = sheet.Format,
            Elements = elements
        };
    }

    public static Sheet Unpack(SheetSchema data)
    {
        var sheet = new Sheet 
        { 
            Name = data.Name,
            Format = data.Format
        };

        foreach (var element in data.Elements)
        {
            sheet.Elements.Add(element.Unpack());
        }

        return sheet;
    }
}
