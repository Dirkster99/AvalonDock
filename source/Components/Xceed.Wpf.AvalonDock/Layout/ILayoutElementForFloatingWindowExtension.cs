namespace Xceed.Wpf.AvalonDock.Layout
{
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
            else
            {
                RECT primaryscreen = new RECT(0, 0, (int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight);

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
        }

        /// <summary>
        /// Determine whether <paramref name="a"/> and <paramref name="b"/>
        /// have an intersection or not.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Determine the place where <paramref name="windowRect"/> should be placed
        /// inside the <paramref name="monitorRect"/>.
        /// </summary>
        /// <param name="monitorRect"></param>
        /// <param name="windowRect"></param>
        /// <returns></returns>
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
