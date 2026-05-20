using System.Windows.Media;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Rendering;

public abstract class SheetElementRenderer : IDisposable
{
    public event Action? InvalidateVisual;

    public abstract SheetElement Element { get; }
    public abstract UnitBounds SelectionBounds { get; }

    public abstract bool HitTest(Unit2D unit);
    public abstract bool BoundsTest(UnitBounds bounds);
    public abstract void Render(DrawingContext dc);

    public abstract void Dispose();

    protected void InvokeInvalidateVisual()
    {
        InvalidateVisual?.Invoke();
    }
}
