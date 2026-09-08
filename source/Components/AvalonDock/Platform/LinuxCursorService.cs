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
		/// <summary>
		/// Queries the X server for the pointer's absolute root-window position.
		/// <para>
		/// This exists because the managed fallback below (<see cref="Mouse.GetPosition(IInputElement)"/>
		/// projected through <see cref="Visual.PointToScreen(Point)"/>) is measured *relative to a
		/// window*, which makes it unusable for the one caller that matters most: dragging a window.
		/// The portable backend applies window moves asynchronously, so while a window is being
		/// dragged its reported origin and the origin a relative position was measured against
		/// disagree, and any position derived from them oscillates - the dragged window visibly
		/// jitters and fights the cursor. XQueryPointer reports the pointer in root (screen) space,
		/// independent of any window, so it stays correct no matter what the dragged window is doing.
		/// </para>
		/// Works on X11 and on Wayland via XWayland; returns false when no X display is reachable
		/// (native Wayland), in which case callers fall back to the managed projection.
		/// </summary>
		private static class X11
		{
			private const string LibX11 = "libX11.so.6";

			private static IntPtr s_display;
			private static bool s_unavailable;

			[System.Runtime.InteropServices.DllImport(LibX11)]
			private static extern IntPtr XOpenDisplay(IntPtr display);

			[System.Runtime.InteropServices.DllImport(LibX11)]
			private static extern IntPtr XDefaultRootWindow(IntPtr display);

			[System.Runtime.InteropServices.DllImport(LibX11)]
			private static extern int XQueryPointer(
				IntPtr display, IntPtr window,
				out IntPtr rootReturn, out IntPtr childReturn,
				out int rootX, out int rootY,
				out int winX, out int winY,
				out uint maskReturn);

			/// <summary>X11 Button1Mask - the physical state of the left button.</summary>
			private const uint Button1Mask = 1 << 8;

			public static bool TryGetLeftButtonDown(out bool isDown)
			{
				isDown = false;
				if (!TryQuery(out _, out _, out var mask)) return false;
				isDown = (mask & Button1Mask) != 0;
				return true;
			}

			public static bool TryGetPointer(out double x, out double y)
				=> TryQuery(out x, out y, out _);

			private static bool TryQuery(out double x, out double y, out uint mask)
			{
				x = 0;
				y = 0;
				mask = 0;
				if (s_unavailable) return false;

				try
				{
					if (s_display == IntPtr.Zero)
					{
						// The display connection is cached: a drag queries this on every mouse move.
						s_display = XOpenDisplay(IntPtr.Zero);
						if (s_display == IntPtr.Zero)
						{
							s_unavailable = true;
							return false;
						}
					}

					var root = XDefaultRootWindow(s_display);
					if (XQueryPointer(s_display, root, out _, out _, out var rootX, out var rootY, out _, out _, out var buttons) == 0)
						return false;

					x = rootX;
					y = rootY;
					mask = buttons;
					return true;
				}
				catch (DllNotFoundException)
				{
					s_unavailable = true;
					return false;
				}
				catch (EntryPointNotFoundException)
				{
					s_unavailable = true;
					return false;
				}
			}
		}

		public (double X, double Y) GetCursorPosition()
		{
			// Prefer the X server's absolute answer; it is the only one that stays valid while the
			// window a drag is moving keeps changing position.
			if (X11.TryGetPointer(out var rootX, out var rootY))
				return (rootX, rootY);

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
		{
			// Prefer the X server's view of the PHYSICAL button. The portable backend can deliver
			// spurious MouseUp events (notably right after a window/popup is shown during a press),
			// which corrupt WPF's Mouse.LeftButton and end drags that the user is still performing.
			// The X11 button mask cannot be faked by a synthesized WPF event.
			if (X11.TryGetLeftButtonDown(out var isDown))
				return isDown;

			return Mouse.LeftButton == MouseButtonState.Pressed;
		}

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
