using System.Windows;
using System.Windows.Controls;

namespace StencilPad.Canvases.Tools.Overlays;

public class ToolOverlay : Decorator
{
    public FrameworkElement? ActiveOverlay
    {
        get => Child as FrameworkElement;
        set => Child = value;
    }
}
