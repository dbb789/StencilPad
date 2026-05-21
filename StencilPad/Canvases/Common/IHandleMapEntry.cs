using StencilPad.Models;
using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public interface IHandleMapEntry : IComparable<IHandleMapEntry>
{
    ISheetElement Element { get; }
    Handle Handle { get; }
    Unit2D Position { get; }
    bool Editing { get; }
    bool Selected { get; }

    void SetPosition(Unit2D position);
    void SetSelected(bool selected);
}
