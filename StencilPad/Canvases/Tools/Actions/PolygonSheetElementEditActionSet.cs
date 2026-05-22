using StencilPad.Canvases.Tools.Actions;
using StencilPad.Canvases.Tools.Common;
using StencilPad.Models;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers.Actions;

public class PolygonSheetElementEditActionSet
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
        return e.PolygonSet.Any(p => (p.Vertices.Count - p.GetSelectedVertices().Count()) > 2);
    };

    private static Func<IPolygonSheetElement, bool> PolygonOpen = e =>
    {
        return e.PolygonSet.Any(p => !p.Closed);
    };

    private static Func<IPolygonSheetElement, bool> CanOpenPolygon = e =>
    {
        return e.PolygonSet.Any(p => (p.GetSelectedEdges().Count() == 1) && p.Closed);
    };

    public IEnumerable<ISheetElementAction?> Actions { get; }

    private class CornerPropertiesAction : ISheetElementAction
    {
        public string Name => "Corner Properties…";

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
            var polygonSheetElements = elements.OfType<IPolygonSheetElement>();
            var targets = new List<VertexCornerTarget>();

            foreach (var element in polygonSheetElements)
            {
                foreach (var polygon in element.PolygonSet)
                {
                    foreach (var vertexIndex in polygon.GetSelectedVertices())
                    {
                        targets.Add(new VertexCornerTarget(element, polygon, vertexIndex));
                    }
                }
            }

            _modelPropertiesService.ShowVertexCornerProperties(sheet, targets);
        }
    }
 
    
    public PolygonSheetElementEditActionSet(IModelPropertiesService modelPropertiesService)
    {
        Actions = [
            new CornerPropertiesAction(modelPropertiesService),
            null,
            new SheetElementAction<IPolygonSheetElement>
            {
                Name = "Insert Point",
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
            },
            new SheetElementAction<IPolygonSheetElement>
            {
                Name = "Delete Points",
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
            },
            null,
            new SheetElementAction<IPolygonSheetElement>
            {
                Name = "Open Path",
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
            },
            new SheetElementAction<IPolygonSheetElement>
            {
                Name = "Close Path",
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
            },
            new SheetElementAction<IPolygonSheetElement>
            {
                Name = "Set As Straight",
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
            },
            new SheetElementAction<IPolygonSheetElement>
            {
                Name = "Set As Curve",
                Enabled = OneOrMoreEdgesSelected,
                Action = e =>
                {
                    foreach (var polygon in e.PolygonSet)
                    {
                        foreach (var edgeIndex in polygon.GetSelectedEdges())
                        {
                            polygon.Edges[edgeIndex] = polygon.Edges[edgeIndex] with { Type = EdgeType.Bezier };
                        }
                    }
                }
            } ];
    }
}
