namespace Xceed.Wpf.AvalonDock.Layout
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;

    /// <summary>
    /// SetWindowPlacement won't correct placement for WPF tool windows
    /// 
    /// https://stackoverflow.com/questions/19203031/setwindowplacement-wont-correct-placement-for-wpf-tool-windows
    /// </summary>
    public static class ILayoutElementForFloatingWindowExtension
    {
        // RECT structure required by WINDOWPLACEMENT structure
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            internal RECT rcMonitor;
            internal RECT rcWork;
            private uint dwFlags;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromRect([In] ref RECT lprc, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        internal static void KeepInsideNearestMonitor(this ILayoutElementForFloatingWindow paneInsideFloatingWindow)
        {
            RECT normalPosition = new RECT();
            normalPosition.Left = (int)paneInsideFloatingWindow.FloatingLeft;
            normalPosition.Top = (int)paneInsideFloatingWindow.FloatingTop;
            normalPosition.Bottom = normalPosition.Top + (int)paneInsideFloatingWindow.FloatingHeight;
            normalPosition.Right = normalPosition.Left + (int)paneInsideFloatingWindow.FloatingWidth;

            // Are we using only one monitor?
            if (SystemParameters.PrimaryScreenWidth == SystemParameters.VirtualScreenWidth &&
                SystemParameters.PrimaryScreenHeight == SystemParameters.VirtualScreenHeight)
            {
                RECT primaryscreen = new RECT(0,0, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);

                if (!RectanglesIntersect(normalPosition, primaryscreen))
                {
                    normalPosition = PlaceOnScreen(primaryscreen, normalPosition);

                    paneInsideFloatingWindow.FloatingLeft = normalPosition.Left;
                    paneInsideFloatingWindow.FloatingTop = normalPosition.Top;
                    paneInsideFloatingWindow.FloatingHeight = normalPosition.Bottom - normalPosition.Top;
                    paneInsideFloatingWindow.FloatingWidth = normalPosition.Right - normalPosition.Left;
                }

                return;
            }

            IntPtr closestMonitorPtr = MonitorFromRect(ref normalPosition, MONITOR_DEFAULTTONEAREST);
            MONITORINFO closestMonitorInfo = new MONITORINFO();
            closestMonitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            bool getInfoSucceeded = GetMonitorInfo(closestMonitorPtr, ref closestMonitorInfo);

            if (getInfoSucceeded && !RectanglesIntersect(normalPosition, closestMonitorInfo.rcMonitor))
            {
                normalPosition = PlaceOnScreen(closestMonitorInfo.rcMonitor, normalPosition);

                paneInsideFloatingWindow.FloatingLeft = normalPosition.Left;
                paneInsideFloatingWindow.FloatingTop = normalPosition.Top;
                paneInsideFloatingWindow.FloatingHeight = normalPosition.Bottom - normalPosition.Top;
                paneInsideFloatingWindow.FloatingWidth = normalPosition.Right - normalPosition.Left;
            }
        }

        private static bool RectanglesIntersect(RECT a, RECT b)
        {
            if (a.Left > b.Right || a.Right < b.Left)
            {
                return false;
            }

            if (a.Top > b.Bottom || a.Bottom < b.Top)
            {
                return false;
            }

            return true;
        }

        private static RECT PlaceOnScreen(RECT monitorRect, RECT windowRect)
        {
            int monitorWidth = monitorRect.Right - monitorRect.Left;
            int monitorHeight = monitorRect.Bottom - monitorRect.Top;

            if (windowRect.Right < monitorRect.Left)
            {
                // Off left side
                int width = windowRect.Right - windowRect.Left;
                if (width > monitorWidth)
                {
                    width = monitorWidth;
                }

                windowRect.Left = monitorRect.Left;
                windowRect.Right = windowRect.Left + width;
            }
            else if (windowRect.Left > monitorRect.Right)
            {
                // Off right side
                int width = windowRect.Right - windowRect.Left;
                if (width > monitorWidth)
                {
                    width = monitorWidth;
                }

                windowRect.Right = monitorRect.Right;
                windowRect.Left = windowRect.Right - width;
            }

            if (windowRect.Bottom < monitorRect.Top)
            {
                // Off top
                int height = windowRect.Bottom - windowRect.Top;
                if (height > monitorHeight)
                {
                    height = monitorHeight;
                }

                windowRect.Top = monitorRect.Top;
                windowRect.Bottom = windowRect.Top + height;
            }
            else if (windowRect.Top > monitorRect.Bottom)
            {
                // Off bottom
                int height = windowRect.Bottom - windowRect.Top;
                if (height > monitorHeight)
                {
                    height = monitorHeight;
                }

                windowRect.Bottom = monitorRect.Bottom;
                windowRect.Top = windowRect.Bottom - height;
            }

            return windowRect;
        }
    }
}
