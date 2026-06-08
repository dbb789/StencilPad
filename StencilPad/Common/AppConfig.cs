using System.Windows.Media;

namespace StencilPad.Common;

public class AppConfig : IAppConfig
{
    public Color GridLineColor { get; set; } = Color.FromRgb(0, 128, 255);
    public Color SelectionColor { get; set; } = Color.FromRgb(0, 0, 255);
    public Color GroupSelectionColor { get; set; } = Color.FromRgb(0, 128, 255);
    public Color MoveHandleColor { get; set; } = Color.FromRgb(255, 128, 0);
    public Color AdjustHandleColor { get; set; } = Color.FromRgb(0, 128, 0);
    
    public double HandleSizePx { get; set; } = 12.0;
    public double PointSnapPx { get; set; } = 32.0;
}
