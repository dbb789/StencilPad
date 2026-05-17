using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public class HandleMapEntry
{
    public ISheetElement Element = null!;
    public IHandleSource Source => Element.HandleSource;
    public Handle Handle;
    public Unit2D Position;
    public bool ElementSelected;
    public bool HandleSelected;

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
