using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public class PolygonSelectionContext
{
    public Polygon Polygon { get; }
    public IEnumerable<int> SelectedVertices { get; }
    public IEnumerable<int> SelectedEdges { get; }

    public PolygonSelectionContext(Polygon polygon,
                                   IEnumerable<int> selectedVertices)
    {
        Polygon = polygon;
        SelectedVertices = selectedVertices;
        SelectedEdges = CalculateSelectedEdges();
    }

    private IEnumerable<int> CalculateSelectedEdges()
    {
        return new List<int>();
    }
}
    
