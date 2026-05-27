using Microsoft.Extensions.DependencyInjection;
using StencilPad.Services;
using StencilPad.Spatial;
using StencilPad.Models;
using StencilPad.Models.Operations;

namespace StencilPad.Canvases.Tools.Actions;

public class SheetElementActionSet
{
    public IEnumerable<ISheetElementAction?> Actions { get; }

    public SheetElementActionSet(IModelPropertiesService modelPropertiesService,
                                 IOperationService operationService)
    {
        Actions = [
            new MultiSheetElementAction<Shape>
            {
                Name = "Shape Properties…",
                Action = (sheet, elements) =>
                {
                    modelPropertiesService.ShowShapeProperties(elements);
                }
            },
            new MultiSheetElementAction<MarkerPath>
            {
                Name = "Marker Path Properties…",
                Action = (sheet, elements) =>
                {
                    modelPropertiesService.ShowMarkerPathProperties(elements);
                }
            },
            new MultiSheetElementAction<Shape>
            {
                Name = "Combine Shapes",
                Enabled = elements => elements.Count() > 1,
                Action = (sheet, elements) =>
                {
                    Shape? newShape = null;

                    foreach (var element in elements)
                    {
                        if (newShape is null)
                        {
                            newShape = element.DeepClone();
                        }
                        else
                        {
                            foreach (var polygon in element.PolygonSet)
                            {
                                var newPolygon = polygon.DeepClone();

                                // Normalise the polygon so that the vertices
                                // are relative to the new shape's current
                                // transform.
                                newPolygon.Transform(newShape.Transform.Invert() * element.Transform);
                                newShape.Add(newPolygon);
                            }
                        }
                    }

                    if (newShape is null)
                    {
                        return;
                    }
                    
                    var operation = new BulkCommandOperation();

                    foreach (var element in elements)
                    {
                        operation.Add(new RemoveSheetElementOperation(sheet, element));
                    }

                    operation.Add(new AddSheetElementOperation(sheet, newShape));

                    operationService.Push(operation);
                }
            },
            null,
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
                                children.Add(child.DeepClone());
                            }
                        }
                        else
                        {
                            children.Add(element.DeepClone());
                        }
                    }

                    var group = new ElementGroup(children);

                    // Watch the ordering here, we want to avoid any issues with
                    // duplicate IDs.
                    foreach (var child in elements)
                    {
                        operation.Add(new RemoveSheetElementOperation(sheet, child));
                    }
                    
                    operation.Add(new AddSheetElementOperation(sheet, group));

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
                            added.Add(child.DeepClone());
                        }

                        operation.Add(new RemoveSheetElementOperation(sheet, group));

                        foreach (var element in added)
                        {
                            element.Transform = group.Transform * element.Transform;
                            operation.Add(new AddSheetElementOperation(sheet, element));
                        }
                    }

                    operationService.Push(operation);

                    foreach (var element in added)
                    {
                        sheet.Selection.Add(element);
                    }
                }
            },
            null,
            new MultiSheetElementAction
            {
                Name = "Mirror X",
                Action = (sheet, elements) =>
                {
                    var editContext = new EditSheetElementContext(sheet, elements);
                    var bounds = GetElementBounds(elements);

                    if (bounds is null)
                    {
                        return;
                    }
                    
                    var centerY = bounds.Value.Center.Y;
                    
                    foreach (var element in elements)
                    {
                        element.MirrorX(centerY);
                    }

                    operationService.Push(editContext.FlushOperation());
                }
            },
            new MultiSheetElementAction
            {
                Name = "Mirror Y",
                Action = (sheet, elements) =>
                {
                    var editContext = new EditSheetElementContext(sheet, elements);
                    var bounds = GetElementBounds(elements);

                    if (bounds is null)
                    {
                        return;
                    }

                    var centerX = bounds.Value.Center.X;

                    foreach (var element in elements)
                    {
                        element.MirrorY(centerX);
                    }

                    operationService.Push(editContext.FlushOperation());
                }
            },
            null,
            new MultiSheetElementAction
            {
                Name = "Justify Top",
                Action = (sheet, elements) =>
                {
                    var editContext = new EditSheetElementContext(sheet, elements);
                    
                    Justify(elements,
                            (selection, element) => new Unit2D(Unit.Zero, selection.Min.Y - element.Min.Y));

                    operationService.Push(editContext.FlushOperation());
                }
            },
            new MultiSheetElementAction
            {
                Name = "Justify Middle",
                Action = (sheet, elements) =>
                {
                    var editContext = new EditSheetElementContext(sheet, elements);
                    
                    Justify(elements,
                            (selection, element) => new Unit2D(Unit.Zero, selection.Center.Y - element.Center.Y));

                    operationService.Push(editContext.FlushOperation());
                }
            },
            new MultiSheetElementAction
            {
                Name = "Justify Bottom",
                Action = (sheet, elements) =>
                {
                    var editContext = new EditSheetElementContext(sheet, elements);
                    
                    Justify(elements,
                            (selection, element) => new Unit2D(Unit.Zero, selection.Max.Y - element.Max.Y));

                    operationService.Push(editContext.FlushOperation());
                }
            },
            null,
            new MultiSheetElementAction
            {
                Name = "Justify Left",
                Action = (sheet, elements) =>
                {
                    var editContext = new EditSheetElementContext(sheet, elements);
                    
                    Justify(elements,
                            (selection, element) => new Unit2D(selection.Min.X - element.Min.X, Unit.Zero));

                    operationService.Push(editContext.FlushOperation());
                }
            },
            new MultiSheetElementAction
            {
                Name = "Justify Centre",
                Action = (sheet, elements) =>
                {
                    var editContext = new EditSheetElementContext(sheet, elements);
                    
                    Justify(elements,
                            (selection, element) => new Unit2D(selection.Center.X - element.Center.X, Unit.Zero));

                    operationService.Push(editContext.FlushOperation());
                }
            },
            new MultiSheetElementAction
            {
                Name = "Justify Right",
                Action = (sheet, elements) =>
                {
                    var editContext = new EditSheetElementContext(sheet, elements);
                    
                    Justify(elements,
                            (selection, element) => new Unit2D(selection.Max.X - element.Max.X, Unit.Zero));

                    operationService.Push(editContext.FlushOperation());
                }
            },
            null,
            new MultiSheetElementAction
            {
                Name = "Bring to Front",
                Action = (sheet, elements) =>
                {
                    var operation = new BulkCommandOperation();

                    foreach (var element in elements)
                    {
                        int index = sheet.Elements.IndexOf(element);
                        
                        operation.Add(new ReorderSheetElementOperation(sheet, index, sheet.Elements.Count - 1));
                    }
                    
                    operationService.Push(operation);
                }
            },
            new MultiSheetElementAction
            {
                Name = "Send to Back",
                Action = (sheet, elements) =>
                {
                    var operation = new BulkCommandOperation();

                    foreach (var element in elements)
                    {
                        int index = sheet.Elements.IndexOf(element);
                        
                        operation.Add(new ReorderSheetElementOperation(sheet, index, 0));
                    }
                    
                    operationService.Push(operation);
                }
            }
        ];
    }
    
    private static void Justify(IEnumerable<ISheetElement> elements,
                                Func<UnitBounds, UnitBounds, Unit2D> getDelta)
    {
        var bounds = GetElementBounds(elements);
        
        if (bounds is null)
        {
            return;
        }
        
        foreach (var element in elements)
        {
            var delta = getDelta(bounds.Value, element.GetTransformedBounds());
            
            element.Transform = element.Transform with
                { Position = element.Transform.Position + delta };
        }
    }

    private static UnitBounds? GetElementBounds(IEnumerable<ISheetElement> elements)
    {
        UnitBounds? bounds = null;

        foreach (var element in elements)
        {
            bounds = UnitBounds.Union(bounds, element.GetTransformedBounds());
        }

        return bounds;
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<SheetElementActionSet>();
    }
}

