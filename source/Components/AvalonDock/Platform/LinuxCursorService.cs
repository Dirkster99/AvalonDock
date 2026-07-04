using System;
using System.Windows;

namespace AvalonDock.Platform
{
	/// <summary>
	/// Linux implementation of ICursorService (stub for future support).
	/// Returns default values - actual implementation would use X11/Wayland.
	/// </summary>
	internal class LinuxCursorService : ICursorService
	{
		public (double X, double Y) GetCursorPosition()
		{
			// TODO: Implement using X11/Wayland
			return (0, 0);
		}

		public bool IsLeftButtonDown()
		{
			// TODO: Implement using X11/Wayland
			return false;
		}

		public (double X, double Y) GetCursorLocationQuartz()
		{
			// On Linux, Quartz coordinates are not applicable
			return GetCursorPosition();
		}

		public (double X, double Y) GetCursorLocationCocoa()
		{
			// On Linux, Cocoa coordinates are not applicable
			return GetCursorPosition();
		}
	}
}