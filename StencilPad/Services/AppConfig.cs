using System.Windows.Media;
using StencilPad.Spatial;

namespace StencilPad.Services;

public class AppConfig
{
    public Color GridLineColor { get; set; } = Color.FromRgb(0, 127, 255);
    public Color SelectionColor { get; set; } = Color.FromRgb(0, 0, 255);
    public Color GroupSelectionColor { get; set; } = Color.FromRgb(255, 0, 255);
    public Color MoveHandleColor { get; set; } = Color.FromRgb(255, 127, 0);
    public Color AdjustHandleColor { get; set; } = Color.FromRgb(0, 127, 0);
    
    public double HandleSizePx { get; set; } = 12.0;
    public double PointSnapPx { get; set; } = 32.0;

    public Unit GridSpacingMetric { get; } = Unit.FromMillimeters(10);
    public int GridSubdivisionsMetric { get; } = 5;
    public Unit GridSpacingImperial { get; } = Unit.FromInches(1);
    public int GridSubdivisionsImperial { get; } = 4;
    public double GridMinSpacingPx { get; } = 8.0;
}
