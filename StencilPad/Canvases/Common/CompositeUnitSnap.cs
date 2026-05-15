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
    
    public Unit2D UnitSnap(Unit2D point, Handle? selfHandle = null)
    {
        foreach (var snap in _snaps)
        {
            point = snap.UnitSnap(point, selfHandle);
        }

        return point;
    }
}
