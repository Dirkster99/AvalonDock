using System;
using System.Windows;

namespace AvalonDock.Platform
{
	/// <summary>
	/// Linux implementation of INativeWindowService (stub for future support).
	/// Returns default values - actual implementation would use X11/Wayland.
	/// </summary>
	internal class LinuxNativeWindowService : INativeWindowService
	{
		public (double X, double Y) GetWindowPosition(IntPtr windowHandle)
		{
			// TODO: Implement using X11/Wayland
			return (0, 0);
		}

		public void SetWindowPosition(IntPtr windowHandle, double x, double y)
		{
			// TODO: Implement using X11/Wayland
		}

		public (double X, double Y, double Width, double Height) GetWindowFrame(IntPtr windowHandle)
		{
			// TODO: Implement using X11/Wayland
			return (0, 0, 0, 0);
		}

		public void BringToFront(IntPtr windowHandle)
		{
			// TODO: Implement using X11/Wayland
		}

		public void CloseWindow(IntPtr windowHandle)
		{
			// TODO: Implement using X11/Wayland
		}

		public void DisableWindowTabbing(IntPtr windowHandle)
		{
			// No-op on Linux
		}

		public void SetWindowAlpha(IntPtr windowHandle, double alpha)
		{
			// TODO: Implement using X11/Wayland
		}

		public void SetWindowLevel(IntPtr windowHandle, int level)
		{
			// TODO: Implement using X11/Wayland
		}

		public (double X, double Y, double Width, double Height) GetContentViewFrame(IntPtr windowHandle)
		{
			// TODO: Implement using X11/Wayland
			return (0, 0, 0, 0);
		}

		public (double X, double Y) GetWindowContentOrigin(IntPtr windowHandle)
		{
			// TODO: Implement using X11/Wayland
			return (0, 0);
		}
	}
}