using System.Windows.Media;

namespace StencilPad.Common;

public interface IAppConfig
{
    Color GridLineColor { get; }
    Color SelectionColor { get; }
    Color GroupSelectionColor { get; }

    double HandleSizePx { get; }
}
