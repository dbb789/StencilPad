using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public interface IHandleMapEntry
{
    ISheetElement Element { get; }
    IHandleSource Source { get; }
    Handle Handle { get; }
    Unit2D Position { get; }
    bool ElementSelected { get; }
    bool HandleSelected { get; }

    void SetPosition(Unit2D position);
    void SetSelected(bool selected);
}
