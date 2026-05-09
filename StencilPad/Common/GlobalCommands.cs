using System.Windows.Input;

namespace StencilPad.Common;

public static class GlobalCommands
{
    public static readonly RoutedUICommand SelectAll =
        new RoutedUICommand("Select All", nameof(SelectAll), typeof(GlobalCommands),
            new InputGestureCollection { new KeyGesture(Key.A, ModifierKeys.Control) });

    public static readonly RoutedUICommand ClearSelection =
        new RoutedUICommand("Clear Selection", nameof(ClearSelection), typeof(GlobalCommands));
}
