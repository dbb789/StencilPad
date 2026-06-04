using System.Windows;
using System.Windows.Controls;

namespace StencilPad.Canvases.Tools.Overlays;

public class OverlayContainer : Decorator
{
    public FrameworkElement? ActiveOverlay
    {
        get => Child as FrameworkElement;
        set => Child = value;
    }
}
