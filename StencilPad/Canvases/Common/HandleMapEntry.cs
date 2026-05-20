using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public class HandleMapEntry : IHandleMapEntry
{
    public IHandleSource Source { get; set; } = null!;
    public Handle Handle { get; set; }
    public Unit2D Position { get; set; }
    public bool Editing { get; set; }
    public bool Selected { get; set; }

    public void SetPosition(Unit2D position)
    {
        if (Position != position)
        {
            Source.SetPoint(Handle, position);
        }
    }
    
    public void SetSelected(bool selected)
    {
        if (Selected != selected)
        {
            Selected = selected;
            Source.SetHandleSelected(Handle, selected);
        }
    }

    public int CompareTo(IHandleMapEntry? other)
    {
        if (other == null)
        {
            return 1;
        }
        
        return Handle.CompareTo(other.Handle);
    }
}
