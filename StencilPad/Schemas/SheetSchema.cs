using StencilPad.Models;

namespace StencilPad.Schemas;

public class SheetSchema
{
    public string Name { get; set; } = string.Empty;
    public SheetFormatSchema Format { get; set; } = new();
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
            Format = SheetFormatSchema.Pack(sheet.Format),
            Elements = elements
        };
    }

    public static Sheet Unpack(SheetSchema data)
    {
        var sheet = new Sheet 
        { 
            Name = data.Name,
            Format = SheetFormatSchema.Unpack(data.Format)
        };

        foreach (var element in data.Elements)
        {
            var sheetElement = element.Unpack();

            sheet.Elements.Add(sheetElement.Id, sheetElement);
        }

        return sheet;
    }
}
