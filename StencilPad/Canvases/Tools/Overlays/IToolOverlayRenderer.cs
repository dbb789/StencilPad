using System.Windows.Media;

namespace StencilPad.Canvases.Tools.Overlays;

public interface IToolOverlayRenderer
{
    event Action? RendererDirty;

    void Render(DrawingContext dc);
    void Dispose();
}
