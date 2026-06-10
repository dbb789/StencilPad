using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Common;

public interface ISettings
{
    event Action? Changed;

    UnitType UnitType { get; }
    
    Color GridLineColor { get; }
    Color SelectionColor { get; }
    Color GroupSelectionColor { get; }
    Color MoveHandleColor { get; }
    Color AdjustHandleColor { get; }

    double HandleSizePx { get; }
    double PointSnapPx { get; }

    Unit GridSpacing { get; }
    int GridSubdivisions { get; }
    double GridMinSpacingPx { get; }
}
