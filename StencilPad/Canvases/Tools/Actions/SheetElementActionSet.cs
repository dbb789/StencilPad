using Microsoft.Extensions.DependencyInjection;
using StencilPad.Services;
using StencilPad.Models;
using StencilPad.Models.Operations;

namespace StencilPad.Canvases.Tools.Actions;

public class SheetElementActionSet
{
    public IEnumerable<ISheetElementAction?> Actions { get; }

    public SheetElementActionSet(IOperationService operationService)
    {
        Actions = [
            new MultiSheetElementAction
            {
                Name = "Group",
                Enabled = elements => elements.Count() > 1,
                Action = (sheet, elements) =>
                {
                    var operation = new BulkCommandOperation();
                    var children = new List<ISheetElement>();

                    foreach (var element in elements)
                    {
                        if (element is ElementGroup)
                        {
                            foreach (var child in ((ElementGroup)element).Children)
                            {
                                children.Add(child);
                            }
                        }
                        else
                        {
                            children.Add(element);
                        }
                    }
                    
                    var group = new ElementGroup(children);

                    operation.Add(new AddSheetElementOperation(sheet, group));
                    
                    foreach (var child in elements)
                    {
                        operation.Add(new RemoveSheetElementOperation(sheet, child));
                    }

                    operationService.Push(operation);

                    sheet.Selection.Add(group);
                }
            },
            new MultiSheetElementAction
            {
                Name = "Ungroup",
                Enabled = elements => elements.Any(e => e is ElementGroup),
                Action = (sheet, elements) =>
                {
                    var groups = elements.OfType<ElementGroup>();
                    var operation = new BulkCommandOperation();
                    var added = new List<ISheetElement>();
                    
                    foreach (var group in groups)
                    {
                        foreach (var child in group.Children)
                        {
                            operation.Add(new AddSheetElementOperation(sheet, child));
                            added.Add(child);
                        }

                        operation.Add(new RemoveSheetElementOperation(sheet, group));
                    }

                    operationService.Push(operation);
                    
                    foreach (var element in added)
                    {
                        sheet.Selection.Add(element);
                    }
                }
            }
        ];
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<SheetElementActionSet>();
    }
}
