using System;
using System.Windows;
using System.Windows.Input;

namespace AvalonDock.Platform
{
	/// <summary>
	/// Linux implementation of <see cref="ICursorService"/> for LibreWPF's portable backend.
	/// <para>
	/// The portable (ProGPU) backend has no user32/X11/Wayland surface to P/Invoke, so the
	/// Windows-style <c>GetCursorPos</c>/<c>GetAsyncKeyState</c> path is unavailable. Instead we
	/// recover the same information from the WPF input stack, which the portable backend pumps:
	/// <see cref="Mouse.GetPosition(IInputElement)"/> relative to a rooted window, projected to
	/// the screen via <see cref="Visual.PointToScreen(Point)"/>, and <see cref="Mouse.LeftButton"/>
	/// for button state.
	/// </para>
	/// </summary>
	internal class LinuxCursorService : ICursorService
	{
		public (double X, double Y) GetCursorPosition()
		{
			var window = LinuxPlatformInterop.GetReferenceWindow();
			if (window == null)
				return (0, 0);

			try
			{
				// Mouse.GetPosition returns coordinates relative to the window; projecting through
				// PointToScreen yields absolute screen (device) coordinates, matching what the
				// Windows service's GetCursorPos returns.
				var relative = Mouse.GetPosition(window);
				var screen = window.PointToScreen(relative);
				return (screen.X, screen.Y);
			}
			catch
			{
				// PointToScreen throws if the window loses its PresentationSource between the
				// rooted check and the projection; treat it as "unknown" rather than crashing a drag.
				return (0, 0);
			}
		}

		public bool IsLeftButtonDown()
			=> Mouse.LeftButton == MouseButtonState.Pressed;

		public (double X, double Y) GetCursorLocationQuartz()
		{
			// Quartz/Cocoa coordinate spaces are macOS concepts; on Linux the single screen-space
			// projection is the best available answer.
			return GetCursorPosition();
		}

		public (double X, double Y) GetCursorLocationCocoa()
		{
			return GetCursorPosition();
		}
	}
}
