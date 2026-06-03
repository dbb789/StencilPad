using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public abstract class SheetElementRenderer : IDisposable
{
    public event Action? RendererDirty;

    public abstract void Render(DrawingContext dc);

    public abstract void Dispose();

    protected void InvokeRendererDirty()
    {
        RendererDirty?.Invoke();
    }
}
