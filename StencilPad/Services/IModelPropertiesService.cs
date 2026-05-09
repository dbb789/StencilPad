using StencilPad.Models;

namespace StencilPad.Services;

public interface IModelPropertiesService
{
    void CloseAll();
    void ShowVertexCornerProperties(Sheet sheet, IReadOnlyList<VertexCornerTarget> targets);
    void ShowMarkerPathProperties(IReadOnlyList<MarkerPath> MarkerPaths);
}
