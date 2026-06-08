using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Common;

public interface IAppConfig
{
    Color GridLineColor { get; }
    Color SelectionColor { get; }
    Color GroupSelectionColor { get; }
    Color MoveHandleColor { get; }
    Color AdjustHandleColor { get; }

    double HandleSizePx { get; }
    double PointSnapPx { get; }

    Unit GridSpacingMetric { get; }
    int GridSubdivisionsMetric { get; }
    Unit GridSpacingImperial { get; }
    int GridSubdivisionsImperial { get; }
    double GridMinSpacingPx { get; }
}
