using System.Windows.Media;

namespace StencilPad.Common;

public class AppConfig : IAppConfig
{
    public Color GridLineColor { get; set; } = Color.FromRgb(0, 128, 255);
    public Color SelectionColor { get; set; } = Color.FromRgb(0, 0, 255);
    public Color GroupSelectionColor { get; set; } = Color.FromRgb(0, 128, 255);
    public double HandleSizePx { get; set; } = 12.0;
}
