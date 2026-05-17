using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public class HandleMapEntry : IHandleMapEntry
{
    public ISheetElement Element { get; set; } = null!;
    public IHandleSource Source => Element.HandleSource;
    public Handle Handle { get; set; }
    public Unit2D Position { get; set; }
    public bool ElementSelected { get; set; }
    public bool HandleSelected { get; set; }

    public void SetPosition(Unit2D position)
    {
        if (Position != position)
        {
            Source.SetPoint(Handle, position);
        }
    }
    
    public void SetSelected(bool selected)
    {
        if (HandleSelected != selected)
        {
            HandleSelected = selected;
            Source.SetHandleSelected(Handle, selected);
        }
    }
}
