using System;
using System.Windows;

namespace AvalonDock.Platform
{
	/// <summary>
	/// Linux implementation of <see cref="INativeWindowService"/> for LibreWPF's portable backend.
	/// <para>
	/// The portable backend exposes no HWND to drive user32 window calls, so each operation is
	/// mapped onto the managed <see cref="Window"/> that owns the supplied handle (resolved via
	/// <see cref="LinuxPlatformInterop.WindowFromHandle"/>). Position/size come from
	/// <see cref="Window.Left"/>/<see cref="Window.Top"/>/<see cref="FrameworkElement.ActualWidth"/>,
	/// z-order from <see cref="Window.Activate"/>/<see cref="Window.Topmost"/>, transparency from
	/// <see cref="Window.Opacity"/>, and screen projection from <see cref="System.Windows.Media.Visual.PointToScreen"/>.
	/// Operations degrade to no-ops (or zero) when the handle cannot be resolved, which is the common
	/// case on the portable backend where the "handle" is a shim value rather than a real window.
	/// </para>
	/// <para>
	/// Why managed rather than X11/Wayland P/Invoke (the macOS services use native objc_msgSend on a
	/// real NSWindow): LibreWPF <em>does</em> ship a native Linux path
	/// (<c>SilkNetWpfWindowDecorationService</c>, an <c>_NET_WM_MOVERESIZE</c> interactive move driven
	/// by <c>libX11.so.6</c>), but it is unsuitable here for two reasons. First, it is X11-only - it
	/// reads <c>INativeWindow.X11</c>, which is null under GLFW's Wayland backend, so it is inert on a
	/// Wayland session. Second, an <c>_NET_WM_MOVERESIZE</c> (or the Wayland <c>xdg_toplevel.move</c>)
	/// move hands the drag to the compositor, which then stops delivering motion events - but AvalonDock
	/// must see every motion event to show its dock overlays and hit-test drop zones (the same reason
	/// the Windows path uses WM_NCLBUTTONDOWN+WM_MOVING, driving its own loop, rather than a compositor
	/// move). Setting <see cref="Window.Left"/>/<see cref="Window.Top"/> is the supported knob: it flows
	/// to the native view (which honors it on X11; GLFW-on-Wayland cannot position a toplevel, but the
	/// host still remembers the requested position so <see cref="System.Windows.Media.Visual.PointToScreen"/>
	/// and drop hit-testing stay correct).
	/// </para>
	/// </summary>
	internal class LinuxNativeWindowService : INativeWindowService
	{
		public (double X, double Y) GetWindowPosition(IntPtr windowHandle)
		{
			var window = LinuxPlatformInterop.WindowFromHandle(windowHandle);
			if (window == null)
				return (0, 0);

			try
			{
				return (window.Left, window.Top);
			}
			catch
			{
				return (0, 0);
			}
		}

		public void SetWindowPosition(IntPtr windowHandle, double x, double y)
		{
			var window = LinuxPlatformInterop.WindowFromHandle(windowHandle);
			if (window == null)
				return;

			try
			{
				window.Left = x;
				window.Top = y;
			}
			catch
			{
				// Setting Left/Top can throw for a window mid-teardown; ignore.
			}
		}

		public (double X, double Y, double Width, double Height) GetWindowFrame(IntPtr windowHandle)
		{
			var window = LinuxPlatformInterop.WindowFromHandle(windowHandle);
			if (window == null)
				return (0, 0, 0, 0);

			try
			{
				return (window.Left, window.Top, OuterWidth(window), OuterHeight(window));
			}
			catch
			{
				return (0, 0, 0, 0);
			}
		}

		public void BringToFront(IntPtr windowHandle)
		{
			var window = LinuxPlatformInterop.WindowFromHandle(windowHandle);
			if (window == null)
				return;

			try
			{
				window.Activate();
			}
			catch
			{
				// Activate throws if the window has already closed; nothing to bring forward.
			}
		}

		public void CloseWindow(IntPtr windowHandle)
		{
			var window = LinuxPlatformInterop.WindowFromHandle(windowHandle);
			if (window == null)
				return;

			try
			{
				window.Close();
			}
			catch
			{
			}
		}

		public void DisableWindowTabbing(IntPtr windowHandle)
		{
			// Window tabbing is a macOS-only concept; nothing to do on Linux.
		}

		public void SetWindowAlpha(IntPtr windowHandle, double alpha)
		{
			var window = LinuxPlatformInterop.WindowFromHandle(windowHandle);
			if (window == null)
				return;

			try
			{
				window.Opacity = Math.Max(0.0, Math.Min(1.0, alpha));
			}
			catch
			{
			}
		}

		public void SetWindowLevel(IntPtr windowHandle, int level)
		{
			var window = LinuxPlatformInterop.WindowFromHandle(windowHandle);
			if (window == null)
				return;

			try
			{
				// WPF exposes only a boolean topmost flag; a positive level maps to topmost,
				// anything else drops back to the normal z-band (there is no "send to bottom").
				window.Topmost = level > 0;
			}
			catch
			{
			}
		}

		public (double X, double Y, double Width, double Height) GetContentViewFrame(IntPtr windowHandle)
		{
			var window = LinuxPlatformInterop.WindowFromHandle(windowHandle);
			if (window == null)
				return (0, 0, 0, 0);

			try
			{
				// AvalonDock floating windows are chromeless, so client size ~= outer size; report it
				// origin-relative to match the Win32 GetClientRect semantics.
				return (0, 0, OuterWidth(window), OuterHeight(window));
			}
			catch
			{
				return (0, 0, 0, 0);
			}
		}

		public (double X, double Y) GetWindowContentOrigin(IntPtr windowHandle)
		{
			var window = LinuxPlatformInterop.WindowFromHandle(windowHandle);
			if (window == null)
				return (0, 0);

			try
			{
				if (PresentationSource.FromVisual(window) != null)
				{
					var origin = window.PointToScreen(new Point(0, 0));
					return (origin.X, origin.Y);
				}
			}
			catch
			{
				// Fall through to the logical Left/Top below.
			}

			try
			{
				return (window.Left, window.Top);
			}
			catch
			{
				return (0, 0);
			}
		}

		private static double OuterWidth(Window window)
		{
			var actual = window.ActualWidth;
			if (actual > 0)
				return actual;
			return double.IsNaN(window.Width) ? 0 : window.Width;
		}

		private static double OuterHeight(Window window)
		{
			var actual = window.ActualHeight;
			if (actual > 0)
				return actual;
			return double.IsNaN(window.Height) ? 0 : window.Height;
		}
	}
}
