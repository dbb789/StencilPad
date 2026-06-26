using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Common;

public interface ISettings
{
    event Action? Changed;

    UnitSystem UnitSystem { get; }
    Fraction UnitRatio { get; }
    UnitSettings UnitSettings { get; }

    Color GridLineColor { get; }
    Color SelectionColor { get; }
    Color GroupSelectionColor { get; }
    Color MoveHandleColor { get; }
    Color AdjustHandleColor { get; }

    double HandleSizePx { get; }
    double PointSnapPx { get; }
    double AngleSnapDegrees { get; }

    Unit GridSpacing { get; }
    int GridSubdivisions { get; }
    double GridMinSpacingPx { get; }
}
