using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Overlays;

public interface IToolOverlayRendererFactory
{
    IToolOverlayRenderer? CreateOverlay(ISheetElement element);
}
