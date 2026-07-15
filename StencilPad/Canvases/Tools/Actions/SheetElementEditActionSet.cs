using Microsoft.Extensions.DependencyInjection;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Actions;

public class SheetElementEditActionSet(IModelPropertiesService modelPropertiesService,
                                       IOperationService operationService)
{
    private static Func<IPolygonSheetElement, bool> OneOrMoreVerticesSelected = e =>
    {
        return e.PolygonSet.Any(p => p.GetSelectedVertices().Count() > 0);
    };

    private static Func<IPolygonSheetElement, bool> OneOrMoreEdgesSelected = e =>
    {
        return e.PolygonSet.Any(p => p.GetSelectedEdges().Count() > 0);
    };

    private static Func<IPolygonSheetElement, bool> CanDeleteVertices = e =>
    {
        return e.PolygonSet.Any(p => (p.Vertices.Count - p.GetSelectedVertices().Count()) > 1);
    };

    private static Func<IPolygonSheetElement, bool> PolygonOpen = e =>
    {
        return e.PolygonSet.Any(p => !p.Closed);
    };

    private static Func<IPolygonSheetElement, bool> CanOpenPolygon = e =>
    {
        return e.PolygonSet.Any(p => (p.GetSelectedEdges().Count() == 1) && p.Closed);
    };

    private class CornerPropertiesAction : ISheetElementAction
    {
        private IModelPropertiesService _modelPropertiesService;

        public CornerPropertiesAction(IModelPropertiesService modelPropertiesService)
        {
            _modelPropertiesService = modelPropertiesService;
        }
        
        public bool IsVisible(Sheet sheet, IEnumerable<ISheetElement> elements)
        {
            return elements.All(e => e is IPolygonSheetElement);
        }

        public bool IsEnabled(Sheet sheet, IEnumerable<ISheetElement> elements)
        {
            var polygonSheetElements = elements.OfType<IPolygonSheetElement>();

            foreach (var polygonSheetElement in polygonSheetElements)
            {
                foreach (var polygon in polygonSheetElement.PolygonSet)
                {
                    if (polygon.GetSelectedVertices().Any())
                    {
                        return true;
                    }
                }
            }
            
            return true;
        }

        public void Invoke(Sheet sheet, IEnumerable<ISheetElement> elements)
        {
            _modelPropertiesService.ShowVertexCornerProperties(sheet);
        }
    }

    public readonly ISheetElementAction CornerProperties = new CornerPropertiesAction(modelPropertiesService);

    public readonly ISheetElementAction InsertPoint = new SheetElementAction<IPolygonSheetElement>(operationService)
    {
        Enabled = OneOrMoreEdgesSelected,
        Action = e =>
        {
            foreach (var polygon in e.PolygonSet)
            {
                foreach (var edgeIndex in polygon.GetSelectedEdges().OrderByDescending(x => x))
                {
                    var start = polygon.Vertices.At(edgeIndex).Position;
                    var end = polygon.Vertices.At(edgeIndex + 1).Position;
                    var vertex = new Vertex((start + end) / 2);

                    polygon.InsertVertex(edgeIndex + 1, vertex);
                }
            }
        }
    };

    public readonly ISheetElementAction DeletePoints = new SheetElementAction<IPolygonSheetElement>(operationService)
    {
        Enabled = e => OneOrMoreVerticesSelected(e) && CanDeleteVertices(e),
        Action = e =>
        {
            foreach (var polygon in e.PolygonSet)
            {
                // Vertex indices are reordered after each deletion, so we need
                // to loop until there are no selected vertices left.
                while (polygon.Vertices.Count > 2)
                {
                    var selectedVertices = polygon.GetSelectedVertices();

                    if (!selectedVertices.Any())
                    {
                        break;
                    }

                    polygon.DeleteVertex(selectedVertices.First());
                }
            }
        }
    };

    public readonly ISheetElementAction OpenPath = new SheetElementAction<IPolygonSheetElement>(operationService)
    {
        Enabled = e => CanOpenPolygon(e),
        Action = e =>
        {
            foreach (var polygon in e.PolygonSet)
            {
                if (polygon.Closed && polygon.GetSelectedEdges().Count() == 1)
                {
                    polygon.Open(polygon.GetSelectedEdges().First());
                }
            }
        }
    };

    public readonly ISheetElementAction ClosePath = new SheetElementAction<IPolygonSheetElement>(operationService)
    {
        Enabled = PolygonOpen,
        Action = e =>
        {
            foreach (var polygon in e.PolygonSet)
            {
                if (!polygon.Closed)
                {
                    polygon.Close();
                }
            }
        }
    };

    public readonly ISheetElementAction SetAsStraight = new SheetElementAction<IPolygonSheetElement>(operationService)
    {
        Enabled = OneOrMoreEdgesSelected,
        Action = e =>
        {
            foreach (var polygon in e.PolygonSet)
            {
                foreach (var edgeIndex in polygon.GetSelectedEdges())
                {
                    polygon.Edges[edgeIndex] = polygon.Edges[edgeIndex] with { Type = EdgeType.Straight };
                }
            }
        }
    };

    public readonly ISheetElementAction SetAsCurve = new SheetElementAction<IPolygonSheetElement>(operationService)
    {
        Enabled = OneOrMoreEdgesSelected,
        Action = e =>
        {
            foreach (var polygon in e.PolygonSet)
            {
                foreach (var edgeIndex in polygon.GetSelectedEdges())
                {
                    var edge = polygon.Edges[edgeIndex];

                    polygon.Edges[edgeIndex] = edge with { Type = EdgeType.Bezier };

                    polygon.CalculateControlPoints(edgeIndex, true);
                }
            }
        }
    };

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<SheetElementEditActionSet>();
    }
}
