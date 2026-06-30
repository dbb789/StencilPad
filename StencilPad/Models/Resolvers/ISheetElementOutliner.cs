using StencilPad.Spatial;

namespace StencilPad.Models.Resolvers;

public interface ISheetElementOutliner
{
    ISheetElement Element { get; }

    event Action? OutlineChanged;

    UnitBounds GetOutlineBounds();
    UnitBounds GetOutlineBounds(UnitTransform transform);
    bool OutlineContainsPoint(Unit2D point);
}
