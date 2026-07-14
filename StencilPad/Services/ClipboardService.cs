using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using StencilPad.Models;
using StencilPad.Models.Operations;
using StencilPad.Schemas;
using StencilPad.Spatial;

namespace StencilPad.Services;

public class ClipboardService : IClipboardService
{
    private const string ClipboardDataFormat = "stencilpad.data";
    private static readonly Unit2D PasteMajorOffset = Unit2D.FromMillimeters(-5, -5);
    private static readonly Unit2D PasteMinorOffset = Unit2D.FromMillimeters(5, -5);

    private readonly IOperationService _operationService;
    
    private int _pasteCounter;

    public ClipboardService(IOperationService operationService)
    {
        _operationService = operationService;
        _pasteCounter = 0;
    }
    
    public void Copy(Sheet sheet)
    {
        _pasteCounter = 0;

        PackToClipboard(sheet, sheet.Selection);
    }

    public void Cut(Sheet sheet)
    {
        Copy(sheet);

        var operations = sheet.Selection
            .Select(e => new RemoveSheetElementOperation(sheet, e));

        _operationService.Push(new BulkCommandOperation(operations));
    }
    
    public void Paste(Sheet sheet)
    {
        var elements = UnpackFromClipboard().ToList();

        if (!elements.Any())
        {
            return;
        }

        ++_pasteCounter;
        
        var pasteOffset = PasteMajorOffset * (_pasteCounter / 10);
        
        pasteOffset += PasteMinorOffset * (_pasteCounter % 10);

        foreach (var element in elements)
        {
            element.Transform = element.Transform with
                { Position = element.Transform.Position + pasteOffset };
        }

        var operations = elements
            .Select(e => new AddSheetElementOperation(sheet, e));

        _operationService.Push(new BulkCommandOperation(operations));

        sheet.Selection.Clear();
        
        foreach (var element in elements)
        {
            sheet.Selection.Add(element);
        }
    }

    private void PackToClipboard(Sheet sheet, IEnumerable<ISheetElement> elements)
    {
        // Pack in render order to preserve z-index when pasting.
        var schemas = elements.OrderBy(e => sheet.Elements.IndexOf(e))
            .Select(SheetElementSchema.Pack)
            .Where(s => s is not null)
            .ToArray();

        Clipboard.SetData(ClipboardDataFormat, JsonSerializer.Serialize(schemas, SchemaJsonOptions.Default));
    }
    
    private IEnumerable<ISheetElement> UnpackFromClipboard()
    {
        if (!Clipboard.ContainsData(ClipboardDataFormat))
        {
            return Enumerable.Empty<ISheetElement>();
        }

        SheetElementSchema[]? schemas;

        try
        {
            schemas = JsonSerializer.Deserialize<SheetElementSchema[]>(Clipboard.GetData(ClipboardDataFormat) as string ?? "",
                                                                       SchemaJsonOptions.Default);
        }
        catch (JsonException je)
        {
            Debug.WriteLine($"Failed to deserialize clipboard content: {je}");
            
            return Enumerable.Empty<ISheetElement>();
        }

        if (schemas is null)
        {
            return Enumerable.Empty<ISheetElement>();
        }

        return schemas.Select(s => s.Unpack());
    }
}
