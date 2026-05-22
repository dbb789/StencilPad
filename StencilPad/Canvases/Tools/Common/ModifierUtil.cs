using System.Windows.Input;

namespace StencilPad.Canvases.Tools.Common;

public static class ModifierUtil
{
    public static bool IsAddingSelection()
    {
        return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
    }

    public static bool IsTogglingSelection()
    {
        return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
    }

    public static bool IsModifyingSelection()
    {
        return IsAddingSelection() || IsTogglingSelection();
    }

    public static bool IsLockToAxis()
    {
        return Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
    }

    public static bool IsLockAspect()
    {
        return Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
    }

    public static bool IsAngleSnap()
    {
        return Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
    }
}
