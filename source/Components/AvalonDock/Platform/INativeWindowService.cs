using System;
using System.Windows;

namespace AvalonDock.Platform
{
	/// <summary>
	/// Platform abstraction for native window operations.
	/// Provides cross-platform support for window positioning, sizing, and management.
	/// </summary>
	internal interface INativeWindowService
	{
		/// <summary>
		/// Gets the window's position in screen coordinates.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		/// <returns>The window position (X, Y) in screen coordinates.</returns>
		(double X, double Y) GetWindowPosition(IntPtr windowHandle);

		/// <summary>
		/// Sets the window's position in screen coordinates.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		/// <param name="x">The X coordinate.</param>
		/// <param name="y">The Y coordinate.</param>
		void SetWindowPosition(IntPtr windowHandle, double x, double y);

		/// <summary>
		/// Gets the window's frame (position and size).
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		/// <returns>The window frame (X, Y, Width, Height).</returns>
		(double X, double Y, double Width, double Height) GetWindowFrame(IntPtr windowHandle);

		/// <summary>
		/// Brings the window to the front.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		void BringToFront(IntPtr windowHandle);

		/// <summary>
		/// Closes the window.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		void CloseWindow(IntPtr windowHandle);

		/// <summary>
		/// Disables window tabbing (macOS specific).
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		void DisableWindowTabbing(IntPtr windowHandle);

		/// <summary>
		/// Sets the window's alpha (transparency).
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		/// <param name="alpha">Alpha value (0.0 to 1.0).</param>
		void SetWindowAlpha(IntPtr windowHandle, double alpha);

		/// <summary>
		/// Sets the window level (z-order).
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		/// <param name="level">The window level.</param>
		void SetWindowLevel(IntPtr windowHandle, int level);

		/// <summary>
		/// Gets the content view's frame.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		/// <returns>The content view frame (X, Y, Width, Height).</returns>
		(double X, double Y, double Width, double Height) GetContentViewFrame(IntPtr windowHandle);

		/// <summary>
		/// Gets the window's content origin in screen coordinates.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		/// <returns>The content origin (X, Y) in screen coordinates.</returns>
		(double X, double Y) GetWindowContentOrigin(IntPtr windowHandle);
	}
}