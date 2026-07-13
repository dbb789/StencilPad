using Microsoft.Extensions.DependencyInjection;
using StencilPad.Services;
using StencilPad.Spatial;
using StencilPad.Models;
using StencilPad.Models.Operations;

namespace StencilPad.Canvases.Tools.Actions;

public class SheetElementActionSet(IModelPropertiesService modelPropertiesService,
                                   IOperationService operationService)
{
    public readonly ISheetElementAction ShapeProperties = new MultiSheetElementAction<Shape>
    {
        Name = "Shape Properties…",
        Action = (sheet, _) =>
        {
            modelPropertiesService.ShowShapeProperties(sheet);
        }
    };

    public readonly ISheetElementAction MarkerPathProperties = new MultiSheetElementAction<MarkerPath>
    {
        Name = "Marker Path Properties…",
        Action = (sheet, _) =>
        {
            modelPropertiesService.ShowMarkerPathProperties(sheet);
        }
    };

    public readonly ISheetElementAction TextProperties = new MultiSheetElementAction<TextElement>
    {
        Name = "Text Properties…",
        Action = (sheet, _) =>
        {
            modelPropertiesService.ShowTextProperties(sheet);
        }
    };

    public readonly ISheetElementAction RulerProperties = new MultiSheetElementAction<Ruler>
    {
        Name = "Ruler Properties…",
        Action = (sheet, _) =>
        {
            modelPropertiesService.ShowRulerProperties(sheet);
        }
    };

    public readonly ISheetElementAction ImageProperties = new MultiSheetElementAction<ImageElement>
    {
        Name = "Image Properties…",
        Action = (sheet, _) =>
        {
            modelPropertiesService.ShowImageProperties(sheet);
        }
    };

    public readonly ISheetElementAction CombineShapes = new MultiSheetElementAction<Shape>
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
    };

    public ISheetElementAction Group = new MultiSheetElementAction
    {
        Name = "Group",
        Enabled = elements => elements.Count() > 1,
        Action = (sheet, elements) =>
        {
            var operation = new BulkCommandOperation();
            var children = new List<ISheetElement>();
            var orderedElements = elements.OrderBy(e => sheet.Elements.IndexOf(e));

            foreach (var element in orderedElements)
            {
                if (element is ElementGroup existingGroup)
                {
                    foreach (var child in existingGroup.Children)
                    {
                        var clone = child.DeepClone();
                        
                        clone.Transform = existingGroup.Transform * clone.Transform;

                        children.Add(clone);
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
    };

    public ISheetElementAction Ungroup = new MultiSheetElementAction
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
    };

    public ISheetElementAction FlipHorizontal = new MultiSheetElementAction
    {
        Name = "Flip Horizontal",
        Action = (sheet, elements) =>
        {
            using var editContext = operationService.CreateEditContext(sheet, elements);
            
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
        }
    };

    public ISheetElementAction FlipVertical = new MultiSheetElementAction
    {
        Name = "Flip Vertical",
        Action = (sheet, elements) =>
        {
            using var editContext = operationService.CreateEditContext(sheet, elements);
            
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
        }
    };

    public ISheetElementAction JustifyTop = new MultiSheetElementAction
    {
        Name = "Justify Top",
        Action = (sheet, elements) =>
        {
            using var editContext = operationService.CreateEditContext(sheet, elements);

            Justify(elements,
                    (selection, element) => new Unit2D(Unit.Zero, selection.Min.Y - element.Min.Y));
        }
    };

    public ISheetElementAction JustifyMiddle = new MultiSheetElementAction
    {
        Name = "Justify Middle",
        Action = (sheet, elements) =>
        {
            using var editContext = operationService.CreateEditContext(sheet, elements);

            Justify(elements,
                    (selection, element) => new Unit2D(Unit.Zero, selection.Center.Y - element.Center.Y));
        }
    };

    public ISheetElementAction JustifyBottom = new MultiSheetElementAction
    {
        Name = "Justify Bottom",
        Action = (sheet, elements) =>
        {
            using var editContext = operationService.CreateEditContext(sheet, elements);

            Justify(elements,
                    (selection, element) => new Unit2D(Unit.Zero, selection.Max.Y - element.Max.Y));
        }
    };

    public ISheetElementAction JustifyLeft = new MultiSheetElementAction
    {
        Name = "Justify Left",
        Action = (sheet, elements) =>
        {
            using var editContext = operationService.CreateEditContext(sheet, elements);

            Justify(elements,
                    (selection, element) => new Unit2D(selection.Min.X - element.Min.X, Unit.Zero));
        }
    };

    public ISheetElementAction JustifyCenter = new MultiSheetElementAction
    {
        Name = "Justify Centre",
        Action = (sheet, elements) =>
        {
            using var editContext = operationService.CreateEditContext(sheet, elements);

            Justify(elements,
                    (selection, element) => new Unit2D(selection.Center.X - element.Center.X, Unit.Zero));
        }
    };

    public ISheetElementAction JustifyRight = new MultiSheetElementAction
    {
        Name = "Justify Right",
        Action = (sheet, elements) =>
        {
            using var editContext = operationService.CreateEditContext(sheet, elements);

            Justify(elements,
                    (selection, element) => new Unit2D(selection.Max.X - element.Max.X, Unit.Zero));
        }
    };

    public ISheetElementAction BringToFront = new MultiSheetElementAction
    {
        Name = "Bring to Front",
        Action = (sheet, elements) =>
        {
            var operation = new BulkCommandOperation();
            var orderedElements = elements.OrderBy(e => sheet.Elements.IndexOf(e));
            int offset = 0;
            
            foreach (var element in orderedElements)
            {
                int index = sheet.Elements.IndexOf(element);

                index -= offset;
                operation.Add(new ReorderSheetElementOperation(sheet, index, sheet.Elements.Count - 1));

                ++offset;
            }
            
            operationService.Push(operation);
        }
    };

    public ISheetElementAction SendToBack = new MultiSheetElementAction
    {
        Name = "Send to Back",
        Action = (sheet, elements) =>
        {
            var operation = new BulkCommandOperation();
            var orderedElements = elements.OrderBy(e => sheet.Elements.IndexOf(e)).Reverse();
            int offset = 0;

            foreach (var element in orderedElements)
            {
                int index = sheet.Elements.IndexOf(element);

                index += offset;
                operation.Add(new ReorderSheetElementOperation(sheet, index, 0));

                ++offset;
            }
            
            operationService.Push(operation);
        }
    };
    
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
            var delta = getDelta(bounds.Value, element.GetBounds());
            
            element.Transform = element.Transform with
                { Position = element.Transform.Position + delta };
        }
    }

    private static UnitBounds? GetElementBounds(IEnumerable<ISheetElement> elements)
    {
        UnitBounds? bounds = null;

        foreach (var element in elements)
        {
            bounds = UnitBounds.Union(bounds, element.GetBounds());
        }

        return bounds;
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<SheetElementActionSet>();
    }
}

