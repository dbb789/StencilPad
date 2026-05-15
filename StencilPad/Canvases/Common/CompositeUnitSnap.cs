using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public class CompositeUnitSnap : IUnitSnap
{
    private List<IUnitSnap> _snaps;

    public CompositeUnitSnap()
    {
        _snaps = [];
    }

    public void Add(IUnitSnap snap)
    {
        _snaps.Add(snap);
    }

    public void Remove(IUnitSnap snap)
    {
        _snaps.Remove(snap);
    }

    public void Clear()
    {
        _snaps.Clear();
    }
    
    public bool TryUnitSnap(Unit2D point, Handle? selfHandle, out Unit2D snappedPoint)
    {
        foreach (var snap in _snaps)
        {
            if (snap.TryUnitSnap(point, selfHandle, out snappedPoint))
            {
                return true;
            }
        }

        snappedPoint = default;

        return false;
    }
}
