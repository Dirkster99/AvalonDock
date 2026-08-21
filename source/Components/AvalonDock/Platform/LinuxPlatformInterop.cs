using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace AvalonDock.Platform
{
	/// <summary>
	/// Shared managed helpers used by the Linux platform services.
	/// <para>
	/// LibreWPF's portable (ProGPU) backend exposes no user32/X11/Wayland surface to
	/// AvalonDock, so the Linux services cannot P/Invoke for cursor, DPI or window state
	/// the way the Windows and macOS services do. Instead they work around the gap using
	/// the WPF input/visual stack - which the portable backend <em>does</em> pump - to
	/// recover the same information in managed code.
	/// </para>
	/// </summary>
	internal static class LinuxPlatformInterop
	{
		/// <summary>
		/// Returns a live, rooted window that can map between WPF and screen coordinates.
		/// Any window connected to a <see cref="PresentationSource"/> produces the same
		/// absolute screen mapping, so the choice only affects which window's input state is
		/// freshest: prefer the active window, then the main window, then any rooted window.
		/// </summary>
		internal static Window GetReferenceWindow()
		{
			var app = Application.Current;
			if (app == null)
				return null;

			Window firstRooted = null;
			try
			{
				foreach (Window w in app.Windows)
				{
					if (!IsRooted(w))
						continue;
					if (w.IsActive)
						return w;
					firstRooted ??= w;
				}
			}
			catch
			{
				// Application.Windows throws if touched off the UI thread; fall through to the
				// MainWindow lookup, which the callers already guard for null.
			}

			if (IsRooted(app.MainWindow))
				return app.MainWindow;

			return firstRooted;
		}

		/// <summary>
		/// Resolves the WPF <see cref="Window"/> that owns a native handle, or <c>null</c> when
		/// the handle is unknown (which is the common case on the portable backend, where the
		/// "handle" is a shim value rather than a real HWND).
		/// </summary>
		internal static Window WindowFromHandle(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return null;

			var source = HwndSource.FromHwnd(handle);
			if (source?.RootVisual is Visual rootVisual)
			{
				var owner = Window.GetWindow(rootVisual);
				if (owner != null)
					return owner;
			}

			var app = Application.Current;
			if (app == null)
				return null;

			try
			{
				foreach (Window w in app.Windows)
				{
					if (w != null && new WindowInteropHelper(w).Handle == handle)
						return w;
				}
			}
			catch
			{
				// Off-thread access or a backend without interop helpers - give up gracefully.
			}

			return null;
		}

		private static bool IsRooted(Window window)
			=> window != null && PresentationSource.FromVisual(window) != null;
	}
}
