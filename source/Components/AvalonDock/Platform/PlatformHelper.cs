using System;
using System.Windows;

namespace AvalonDock.Platform
{
	/// <summary>
	/// Platform-agnostic helper methods for common operations.
	/// Replaces direct Win32Helper calls with platform-abstracted operations.
	/// </summary>
	internal static class PlatformHelper
	{
		/// <summary>
		/// Gets the current cursor position in screen coordinates.
		/// </summary>
		internal static Point GetCursorPosition()
		{
			var (x, y) = PlatformManager.CursorService.GetCursorPosition();
			return new Point(x, y);
		}

		/// <summary>
		/// Gets whether the left mouse button is pressed.
		/// </summary>
		internal static bool IsLeftButtonDown()
		{
			return PlatformManager.CursorService.IsLeftButtonDown();
		}

		/// <summary>
		/// Gets the window position in screen coordinates.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		internal static Point GetWindowPosition(IntPtr windowHandle)
		{
			var (x, y) = PlatformManager.NativeWindowService.GetWindowPosition(windowHandle);
			return new Point(x, y);
		}

		/// <summary>
		/// Brings the window to the front.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		internal static void BringWindowToFront(IntPtr windowHandle)
		{
			PlatformManager.NativeWindowService.BringToFront(windowHandle);
		}

		/// <summary>
		/// Disables window tabbing (macOS specific).
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		internal static void DisableWindowTabbing(IntPtr windowHandle)
		{
			PlatformManager.NativeWindowService.DisableWindowTabbing(windowHandle);
		}

		/// <summary>
		/// Sets the window's alpha (transparency).
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		/// <param name="alpha">Alpha value (0.0 to 1.0).</param>
		internal static void SetWindowAlpha(IntPtr windowHandle, double alpha)
		{
			PlatformManager.NativeWindowService.SetWindowAlpha(windowHandle, alpha);
		}

		/// <summary>
		/// Sets the window level (z-order).
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		/// <param name="level">The window level.</param>
		internal static void SetWindowLevel(IntPtr windowHandle, int level)
		{
			PlatformManager.NativeWindowService.SetWindowLevel(windowHandle, level);
		}

		/// <summary>
		/// Closes the window.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		internal static void CloseWindow(IntPtr windowHandle)
		{
			PlatformManager.NativeWindowService.CloseWindow(windowHandle);
		}

		/// <summary>
		/// Gets the DPI scaling factor for the primary monitor.
		/// </summary>
		internal static double GetPrimaryMonitorDpi()
		{
			return PlatformManager.DpiService.GetPrimaryMonitorDpi();
		}

		/// <summary>
		/// Gets the DPI scaling factor for the monitor containing the specified window.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		internal static double GetMonitorDpi(IntPtr windowHandle)
		{
			return PlatformManager.DpiService.GetMonitorDpi(windowHandle);
		}

		/// <summary>
		/// Gets the work area (excluding taskbar) of the primary monitor.
		/// </summary>
		internal static Rect GetPrimaryMonitorWorkArea()
		{
			return PlatformManager.DpiService.GetPrimaryMonitorWorkArea();
		}

		/// <summary>
		/// Gets the work area (excluding taskbar) of the monitor containing the specified window.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		internal static Rect GetMonitorWorkArea(IntPtr windowHandle)
		{
			return PlatformManager.DpiService.GetMonitorWorkArea(windowHandle);
		}
	}
}