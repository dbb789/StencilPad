using System.Windows.Media;

namespace StencilPad.Rendering;

public interface IWalkerRenderer : IDisposable
{
    event Action? RendererDirty;
    
    void Render(DrawingContext dc);
}
