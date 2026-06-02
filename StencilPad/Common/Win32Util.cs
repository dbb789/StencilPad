using System.Runtime.InteropServices;
using System.Windows;

namespace StencilPad.Common;

public static class Win32Util
{
    // Returns the work area (screen minus taskbar) of the monitor that contains
    // the given point, in WPF logical pixels.  Falls back to the primary monitor
    // work area if the monitor cannot be determined.
    //
    // devicePoint must be in device (physical) pixels — e.g. from PointToScreen().
    public static Rect GetWorkAreaForDevicePoint(Point devicePoint, PresentationSource? presentationSource)
    {
        var hMonitor = MonitorFromPoint(new POINT((int)devicePoint.X, (int)devicePoint.Y),
                                        MONITOR_DEFAULTTONEAREST);

        var info = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };

        if (GetMonitorInfo(hMonitor, ref info))
        {
            var wa = info.rcWork;
            var deviceRect = new Rect(wa.left, wa.top,
                                      wa.right  - wa.left,
                                      wa.bottom - wa.top);

            return presentationSource != null
                ? TransformFromDevice(deviceRect, presentationSource)
                : deviceRect;
        }

        return SystemParameters.WorkArea;
    }

    private static Rect TransformFromDevice(Rect deviceRect, PresentationSource source)
    {
        var m = source.CompositionTarget.TransformFromDevice;

        var topLeft     = m.Transform(new Point(deviceRect.Left,  deviceRect.Top));
        var bottomRight = m.Transform(new Point(deviceRect.Right, deviceRect.Bottom));

        return new Rect(topLeft, bottomRight);
    }

    // -------------------------------------------------------------------------
    // P/Invoke declarations
    // -------------------------------------------------------------------------

    private const int MONITOR_DEFAULTTONEAREST = 2;

    [DllImport("user32.dll")]
    private static extern nint MonitorFromPoint(POINT pt, int dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(nint hMonitor, ref MONITORINFO lpmi);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT(int x, int y)
    {
        public int X = x;
        public int Y = y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left, top, right, bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MONITORINFO
    {
        public int  cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }
}
