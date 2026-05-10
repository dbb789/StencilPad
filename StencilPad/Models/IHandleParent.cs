using StencilPad.Spatial;

namespace StencilPad.Models;

public interface IHandleParent
{
    Unit2D GetPoint(Handle handle);
    void SetPoint(Handle handle, Unit2D position);
}
