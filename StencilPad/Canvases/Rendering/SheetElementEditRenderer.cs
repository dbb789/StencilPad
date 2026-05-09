using System.Windows.Media;

namespace StencilPad.Canvases.Rendering;

public abstract class SheetElementEditRenderer : IDisposable
{
    public event Action? InvalidateVisual;

    public abstract void Render(DrawingContext dc);
    public abstract void Dispose();

    protected void InvokeInvalidateVisual()
    {
        InvalidateVisual?.Invoke();
    }
}
