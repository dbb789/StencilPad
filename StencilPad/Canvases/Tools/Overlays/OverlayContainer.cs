using System.Windows;
using System.Windows.Controls;

namespace StencilPad.Canvases.Tools.Overlays;

public class OverlayContainer : Decorator
{
    public FrameworkElement? ActiveOverlay
    {
        get => _child;
        set => SetChild(value);
    }

    private FrameworkElement? _child;

    private void SetChild(FrameworkElement? newChild)
    {
        if (_child is not null)
        {
            _child.Loaded -= ChildLoaded;
        }

        _child = newChild;
        Child = _child;

        if (_child is not null)
        {
            _child.Focusable = true;
            _child.Focus();
            _child.Loaded += ChildLoaded;
        }
    }

    private void ChildLoaded(object sender, RoutedEventArgs e)
    {
        _child?.Focus();
    }
}
