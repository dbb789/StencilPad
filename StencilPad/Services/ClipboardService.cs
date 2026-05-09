using System.Text.Json;
using System.Windows;
using StencilPad.Models;
using StencilPad.Schemas;

namespace StencilPad.Services;

public class ClipboardService : IClipboardService
{
    private static readonly JsonSerializerOptions JsonOptions = SchemaJsonOptions.Default;

    public void Copy(IEnumerable<ISheetElement> elements)
    {
        var schemas = elements
            .Select(SheetElementSchema.Pack)
            .Where(s => s is not null)
            .ToArray();

        Clipboard.SetText(JsonSerializer.Serialize(schemas, JsonOptions));
    }

    public IReadOnlyList<ISheetElement> Paste()
    {
        if (!Clipboard.ContainsText())
        {
            return [];
        }

        SheetElementSchema[]? schemas;

        try
        {
            schemas = JsonSerializer.Deserialize<SheetElementSchema[]>(Clipboard.GetText(), JsonOptions);
        }
        catch (JsonException)
        {
            return [];
        }

        if (schemas is null)
        {
            return [];
        }

        return schemas.Select(s => s.Unpack()).ToList();
    }
}
