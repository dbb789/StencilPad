using StencilPad.Models;

namespace StencilPad.Services;

public interface IModelPropertiesService
{
    void CloseAll();
    void ShowVertexCornerProperties(Sheet sheet);
    void ShowMarkerPathProperties(Sheet sheet);
    void ShowShapeProperties(Sheet sheet);
    void ShowTextProperties(Sheet sheet);
    void ShowRulerProperties(Sheet sheet);
}
