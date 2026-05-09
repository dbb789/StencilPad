using StencilPad.Models;

namespace StencilPad.Schemas;

public class SheetSchema
{
    public string Name { get; set; } = string.Empty;
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
            Elements = elements
        };
    }

    public static Sheet Unpack(SheetSchema data)
    {
        var sheet = new Sheet { Name = data.Name };

        foreach (var element in data.Elements)
        {
            sheet.Elements.Add(element.Unpack());
        }

        return sheet;
    }
}
