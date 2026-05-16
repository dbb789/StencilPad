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
    
    public Unit2D? UnitSnap(Unit2D position, IUnitSnapContext context)
    {
        foreach (var snap in _snaps)
        {
            var snapPosition = snap.UnitSnap(position, context);

            if (snapPosition != null)
            {
                return snapPosition;
            }
        }

        return null;
    }
}
