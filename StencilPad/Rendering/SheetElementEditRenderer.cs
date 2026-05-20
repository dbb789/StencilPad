using System.Windows.Media;

namespace StencilPad.Rendering;

public abstract class SheetElementEditRenderer : IDisposable
{
    public event Action? RendererDirty;

    public abstract void Render(DrawingContext dc);
    public abstract void Dispose();

    protected void InvokeRendererDirty()
    {
        RendererDirty?.Invoke();
    }
}
