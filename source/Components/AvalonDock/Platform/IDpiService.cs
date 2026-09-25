using System;
using System.Windows;

namespace AvalonDock.Platform
{
	/// <summary>
	/// Platform abstraction for DPI operations.
	/// Provides cross-platform support for DPI scaling and monitor information.
	/// </summary>
	internal interface IDpiService
	{
		/// <summary>
		/// Gets the DPI scaling factor for the primary monitor.
		/// </summary>
		/// <returns>The DPI scaling factor.</returns>
		double GetPrimaryMonitorDpi();

		/// <summary>
		/// Gets the DPI scaling factor for the monitor containing the specified window.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		/// <returns>The DPI scaling factor.</returns>
		double GetMonitorDpi(IntPtr windowHandle);

		/// <summary>
		/// Gets the work area (excluding taskbar) of the primary monitor.
		/// </summary>
		/// <returns>The work area rectangle.</returns>
		Rect GetPrimaryMonitorWorkArea();

		/// <summary>
		/// Gets the work area (excluding taskbar) of the monitor containing the specified window.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		/// <returns>The work area rectangle.</returns>
		Rect GetMonitorWorkArea(IntPtr windowHandle);
	}
}