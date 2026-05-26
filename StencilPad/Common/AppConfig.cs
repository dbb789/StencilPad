using System.Windows.Media;

namespace StencilPad.Common;

public class AppConfig : IAppConfig
{
    public Color GridLineColor { get; set; } = Color.FromRgb(0, 128, 255);
}
