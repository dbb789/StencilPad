using StencilPad.Models;

namespace StencilPad.Services;

public interface IModelPropertiesService
{
    void CloseAll();
    void ShowVertexCornerProperties(Sheet sheet, IEnumerable<VertexCornerTarget> targets);
    void ShowMarkerPathProperties(IEnumerable<MarkerPath> MarkerPaths);
}
