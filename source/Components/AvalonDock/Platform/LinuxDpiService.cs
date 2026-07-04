using System;
using System.Windows;

namespace AvalonDock.Platform
{
	/// <summary>
	/// Linux implementation of IDpiService (stub for future support).
	/// Returns default values - actual implementation would use X11/Wayland.
	/// </summary>
	internal class LinuxDpiService : IDpiService
	{
		public double GetPrimaryMonitorDpi()
		{
			// TODO: Implement using X11/Wayland
			return 1.0;
		}

		public double GetMonitorDpi(IntPtr windowHandle)
		{
			// TODO: Implement using X11/Wayland
			return 1.0;
		}

		public Rect GetPrimaryMonitorWorkArea()
		{
			// TODO: Implement using X11/Wayland
			return new Rect(0, 0, 1920, 1080);
		}

		public Rect GetMonitorWorkArea(IntPtr windowHandle)
		{
			// TODO: Implement using X11/Wayland
			return new Rect(0, 0, 1920, 1080);
		}
	}
}