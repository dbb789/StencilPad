using StencilPad.Models;
using StencilPad.Services;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Controllers.Actions;

public class PolygonSheetElementEditActions
{
    private static Func<IPolygonSheetElement, bool> OneOrMoreVerticesSelected = e =>
    {
        return e.EditablePolygon.GetSelectedVertices().Count() > 0;
    };

    private static Func<IPolygonSheetElement, bool> OneEdgeSelected = e =>
    {
        return e.EditablePolygon.GetSelectedEdges().Count() == 1;
    };

    private static Func<IPolygonSheetElement, bool> OneOrMoreEdgesSelected = e =>
    {
        return e.EditablePolygon.GetSelectedEdges().Count() > 0;
    };

    private static Func<IPolygonSheetElement, bool> CanDeleteVertices = e =>
    {
        return (e.EditablePolygon.Vertices.Count - e.EditablePolygon.GetSelectedVertices().Count()) > 2;
    };

    private static Func<IPolygonSheetElement, bool> PolygonOpen = e =>
    {
        return !e.EditablePolygon.Closed;
    };

    private static Func<IPolygonSheetElement, bool> PolygonClosed = e =>
    {
        return e.EditablePolygon.Closed;
    };

    public IEnumerable<ISheetElementAction> Actions { get; }

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
            return elements.OfType<IPolygonSheetElement>().Any();
        }

        public bool IsEnabled(Sheet sheet, IEnumerable<ISheetElement> elements)
        {
            var polygonSheetElements = elements.OfType<IPolygonSheetElement>();

            foreach (var polygonSheetElement in polygonSheetElements)
            {
                var polygon = polygonSheetElement.EditablePolygon;

                if (polygon.GetSelectedVertices().Any())
                {
                    return true;
                }
            }
            
            return true;
        }

        public void Invoke(Sheet sheet, IEnumerable<ISheetElement> elements)
        {
            var polygonSheetElements = elements.OfType<IPolygonSheetElement>();
            var targets = new List<VertexCornerTarget>();

            foreach (var polygonSheetElement in polygonSheetElements)
            {
                var polygon = polygonSheetElement.EditablePolygon;

                foreach (var vertexIndex in polygon.GetSelectedVertices())
                {
                    targets.Add(new VertexCornerTarget(polygonSheetElement, vertexIndex));
                }
            }

            _modelPropertiesService.ShowVertexCornerProperties(sheet, targets);
        }
    }
 
    
    public PolygonSheetElementEditActions(IModelPropertiesService modelPropertiesService)
    {
        Actions = [
            new CornerPropertiesAction(modelPropertiesService),
            new SheetElementAction<IPolygonSheetElement>
            {
                Name = "Insert Point",
                Enabled = OneOrMoreEdgesSelected,
                Action = e =>
                {
                    var polygon = e.EditablePolygon;

                    foreach (var edgeIndex in polygon.GetSelectedEdges().OrderByDescending(x => x))
                    {
                        var start = polygon.Vertices.At(edgeIndex).Position;
                        var end = polygon.Vertices.At(edgeIndex + 1).Position;
                        var vertex = new Vertex((start + end) / 2);

                        polygon.InsertVertex(edgeIndex + 1, vertex);
                    }
                }
            },
            new SheetElementAction<IPolygonSheetElement>
            {
                Name = "Delete Points",
                Enabled = e => OneOrMoreVerticesSelected(e) && CanDeleteVertices(e),
                Action = e =>
                {
                    var polygon = e.EditablePolygon;
                    
                    // Vertex indices are reordered after each deletion, so we need
                    // to loop until there are no selected vertices left.
                    while (true)
                    {
                        var selectedVertices = polygon.GetSelectedVertices();

                        if (!selectedVertices.Any())
                        {
                            break;
                        }

                        polygon.DeleteVertex(selectedVertices.First());
                    }
                }
            },
            new SheetElementAction<IPolygonSheetElement>
            {
                Name = "Open Path",
                Enabled = e => OneEdgeSelected(e) && PolygonClosed(e),
                Action = e =>
                {
                    var polygon = e.EditablePolygon;

                    polygon.Open(polygon.GetSelectedEdges().First());
                }
            },
            new SheetElementAction<IPolygonSheetElement>
            {
                Name = "Close Path",
                Enabled = PolygonOpen,
                Action = e => e.EditablePolygon.Close()
            },
            new SheetElementAction<IPolygonSheetElement>
            {
                Name = "Set As Straight",
                Enabled = OneOrMoreEdgesSelected,
                Action = e =>
                {
                    var polygon = e.EditablePolygon;

                    foreach (var edgeIndex in polygon.GetSelectedEdges())
                    {
                        polygon.Edges[edgeIndex] = polygon.Edges[edgeIndex] with { Type = EdgeType.Straight };
                    }
                }
            },
            new SheetElementAction<IPolygonSheetElement>
            {
                Name = "Set As Curve",
                Enabled = OneOrMoreEdgesSelected,
                Action = e =>
                {
                    var polygon = e.EditablePolygon;

                    foreach (var edgeIndex in polygon.GetSelectedEdges())
                    {
                        polygon.Edges[edgeIndex] = polygon.Edges[edgeIndex] with { Type = EdgeType.Bezier };
                    }
                }
            } ];
    }
}
