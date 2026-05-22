using System.Windows.Input;

namespace StencilPad.Canvases.Tools.Common;

public static class ModifierUtil
{
    public static bool IsModifyingSelection()
    {
        return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
    }

    public static bool IsLockToAxis()
    {
        return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
    }

    public static bool IsLockAspect()
    {
        return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
    }

    public static bool IsAngleSnap()
    {
        return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
    }
}
