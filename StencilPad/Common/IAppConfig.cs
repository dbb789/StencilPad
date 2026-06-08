using System.Windows.Media;

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
}
