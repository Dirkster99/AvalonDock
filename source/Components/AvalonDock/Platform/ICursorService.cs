using System;
using System.Windows;

namespace AvalonDock.Platform
{
	/// <summary>
	/// Platform abstraction for cursor operations.
	/// Provides cross-platform support for cursor positioning and state.
	/// </summary>
	internal interface ICursorService
	{
		/// <summary>
		/// Gets the current cursor position in screen coordinates.
		/// </summary>
		/// <returns>The cursor position (X, Y) in screen coordinates.</returns>
		(double X, double Y) GetCursorPosition();

		/// <summary>
		/// Gets whether the left mouse button is pressed.
		/// </summary>
		/// <returns>True if the left button is pressed.</returns>
		bool IsLeftButtonDown();

		/// <summary>
		/// Gets the cursor position in Quartz coordinates (macOS).
		/// </summary>
		/// <returns>The cursor position in Quartz coordinates.</returns>
		(double X, double Y) GetCursorLocationQuartz();

		/// <summary>
		/// Gets the cursor position in Cocoa coordinates (macOS).
		/// </summary>
		/// <returns>The cursor position in Cocoa coordinates.</returns>
		(double X, double Y) GetCursorLocationCocoa();
	}
}