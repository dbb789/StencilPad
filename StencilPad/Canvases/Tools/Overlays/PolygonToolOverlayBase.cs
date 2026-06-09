using System.Windows.Controls;
using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Tools.Overlays;

public abstract class PolygonToolOverlayBase<TSheetElement> : Canvas, IDisposable
    where TSheetElement : IPolygonSheetElement, new()
{
    public abstract TSheetElement Element { get; }

    public event Action<Polygon>? OnPolygonCompleted;

    protected void InvokePolygonCompleted(Polygon polygon)
    {
        OnPolygonCompleted?.Invoke(polygon);
    }

    public abstract void Dispose();
}
