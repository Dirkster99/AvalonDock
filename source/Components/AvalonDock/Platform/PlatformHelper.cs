using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
		/// Gets a value indicating whether screen coordinates reported for the visual are logical units
		/// (points) rather than device pixels.
		/// </summary>
		/// <remarks>
		/// LibreWPF documents this contract in <c>PointUtil</c>: a portable presentation source reports
		/// screen and client coordinates in the platform's logical coordinate space, the same space as
		/// <c>Window.Left</c>/<c>Top</c> and input events, and keeps <c>TransformToDevice</c> on the
		/// rendering target only; a native Windows window keeps the WPF contract of device pixels.
		/// The criterion is therefore the kind of source, but LibreWPF exposes no public way to ask:
		/// <c>PresentationSource.FromVisual</c> returns an <see cref="System.Windows.Interop.HwndSource"/>
		/// on macOS too (a portable facade whose <c>IsPortable</c> is internal). Until LibreWPF offers one
		/// (tracked in docs/librewpf.md, section 11.10) this is decided by the operating system, which is
		/// what the portable lane equals today: every non-Windows window is portable, every Windows window
		/// is native. Real WPF always reports device pixels.
		/// </remarks>
		/// <param name="visual">The visual the coordinates belong to (kept for the day the source decides).</param>
		/// <returns><c>true</c> when <c>PointToScreen</c> results are already in logical units.</returns>
		internal static bool UsesLogicalScreenCoordinates(System.Windows.Media.Visual visual)
		{
#if LIBREWPF
			return visual != null && !System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
#else
			return false;
#endif
		}

		private sealed class NativeWindowHandle
		{
			internal NativeWindowHandle(IntPtr value) => Value = value;
			internal IntPtr Value { get; }
		}

#if LIBREWPF
		private static readonly ConditionalWeakTable<Window, NativeWindowHandle> NativeWindowHandles = new();
		private static readonly Dictionary<IntPtr, WeakReference<Window>> NativeWindowOwners = new();
#endif

		/// <summary>Remembers the native handle of a window so it can be resolved back to the window.</summary>
		/// <param name="window">The window to remember.</param>
		/// <returns><c>true</c> when a handle was cached.</returns>
		internal static bool CacheNativeWindowHandle(Window window)
		{
#if !LIBREWPF
			// ProGPU types only exist in the LibreWPF build. This path is macOS-only, so on the
			// Windows build (upstream WPF) there is nothing to cache.
			return false;
#else
			if (window == null || !System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
				System.Runtime.InteropServices.OSPlatform.OSX))
				return false;
			if (NativeWindowHandles.TryGetValue(window, out _))
				return true;
			if (!System.Windows.Media.ProGPU.ProGpuWpfDiagnostics.TryGetWindowHost(window, out var host) ||
				host?.SilkWindow?.Native is not { } native || native.Cocoa is not { } cocoa || cocoa == IntPtr.Zero)
				return false;
			lock (NativeWindowOwners)
			{
				if (NativeWindowOwners.TryGetValue(cocoa, out var existing) &&
					existing.TryGetTarget(out var owner) && !ReferenceEquals(owner, window) && owner.IsLoaded)
					return false;
				NativeWindowOwners[cocoa] = new WeakReference<Window>(window);
			}
			NativeWindowHandles.Remove(window);
			NativeWindowHandles.Add(window, new NativeWindowHandle(cocoa));
			return true;
#endif
		}

		/// <summary>
		/// Gets the current cursor position in screen coordinates.
		/// </summary>
		/// <returns><c>true</c> when the left mouse button is pressed.</returns>
		internal static Point GetCursorPosition()
		{
			var (x, y) = PlatformManager.CursorService.GetCursorPosition();
			return new Point(x, y);
		}

		/// <summary>
		/// Gets whether the left mouse button is pressed.
		/// </summary>
		/// <returns>The cursor position in screen coordinates.</returns>
		internal static bool IsLeftButtonDown()
		{
			return PlatformManager.CursorService.IsLeftButtonDown();
		}

		/// <summary>
		/// Gets the window position in screen coordinates.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		/// <returns>The window position in screen coordinates.</returns>
		internal static Point GetWindowPosition(IntPtr windowHandle)
		{
			var (x, y) = PlatformManager.NativeWindowService.GetWindowPosition(windowHandle);
			return new Point(x, y);
		}

		/// <summary>Moves a window to a screen position through the platform's native window service.</summary>
		/// <param name="window">The window to move.</param>
		/// <param name="x">The left edge in screen coordinates.</param>
		/// <param name="y">The top edge in screen coordinates.</param>
		internal static void SetWindowPosition(Window window, double x, double y)
		{
			var windowHandle = GetNativeWindowHandle(window);
			if (windowHandle != IntPtr.Zero)
			{
				PlatformManager.NativeWindowService.SetWindowPosition(windowHandle, x, y);
				return;
			}

			// Not every portable window has a native handle to move: a chromeless floating window on
			// Linux and every overlay window on macOS report none. For those the managed position IS
			// the window position - Window.Left/Top forwards to the portable window host - so a
			// silently dropped move would leave the window behind the pointer for the whole drag.
			try
			{
				window.Left = x;
				window.Top = y;
			}
			catch (InvalidOperationException)
			{
				// Mid-teardown; the window is going away anyway.
			}
		}

		/// <summary>Gets the screen position of the window's content area (inside the frame).</summary>
		/// <param name="window">The window to measure.</param>
		/// <returns>The screen position of the top-left corner of the content area.</returns>
		internal static Point GetWindowContentOrigin(Window window)
		{
			var windowHandle = GetNativeWindowHandle(window);
			if (windowHandle == IntPtr.Zero)
				return new Point(window.Left, window.Top);
			var (x, y) = PlatformManager.NativeWindowService.GetWindowContentOrigin(windowHandle);
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
		/// Resolves the native window handle that <see cref="INativeWindowService"/> expects.
		/// </summary>
		/// <remarks>
		/// WindowInteropHelper only yields a usable handle on Windows. Under LibreWPF on macOS it
		/// returns an HWND-shaped surrogate rather than the NSWindow pointer the Cocoa service sends
		/// Objective-C messages to, so the genuine handle has to come from the ProGPU window host.
		/// Other platforms keep the WindowInteropHelper value, which is what they used before.
		/// </remarks>
		/// <param name="window">The window whose native handle is needed.</param>
		/// <returns>The native window handle.</returns>
		internal static IntPtr GetNativeWindowHandle(Window window)
		{
			if (window == null)
				return IntPtr.Zero;

#if LIBREWPF
			if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
				System.Runtime.InteropServices.OSPlatform.OSX))
			{
				var cacheHandle = window is not global::AvalonDock.Controls.OverlayWindow
					and not global::AvalonDock.Controls.LayoutFloatingWindowControl;
				if (NativeWindowHandles.TryGetValue(window, out var cached))
					return cached.Value;
				if (window is global::AvalonDock.Controls.OverlayWindow)
					return IntPtr.Zero;

				var handle = System.Windows.Media.ProGPU.ProGpuWpfDiagnostics.TryGetWindowHost(window, out var host) &&
					host?.SilkWindow?.Native is { } native &&
					native.Cocoa is { } cocoa
					? cocoa
					: IntPtr.Zero;
				if (cacheHandle && handle != IntPtr.Zero)
				{
					return CacheNativeWindowHandle(window) ? handle : IntPtr.Zero;
				}
				return handle;
			}
#endif

			return new System.Windows.Interop.WindowInteropHelper(window).Handle;
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
		/// <returns>The DPI scaling factor.</returns>
		internal static double GetPrimaryMonitorDpi()
		{
			return PlatformManager.DpiService.GetPrimaryMonitorDpi();
		}

		/// <summary>
		/// Gets the DPI scaling factor for the monitor containing the specified window.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		/// <returns>The DPI scaling factor.</returns>
		internal static double GetMonitorDpi(IntPtr windowHandle)
		{
			return PlatformManager.DpiService.GetMonitorDpi(windowHandle);
		}

		/// <summary>
		/// Gets the work area (excluding taskbar) of the primary monitor.
		/// </summary>
		/// <returns>The work area in screen coordinates.</returns>
		internal static Rect GetPrimaryMonitorWorkArea()
		{
			return PlatformManager.DpiService.GetPrimaryMonitorWorkArea();
		}

		/// <summary>
		/// Gets the work area (excluding taskbar) of the monitor containing the specified window.
		/// </summary>
		/// <param name="windowHandle">The native window handle.</param>
		/// <returns>The work area in screen coordinates.</returns>
		internal static Rect GetMonitorWorkArea(IntPtr windowHandle)
		{
			return PlatformManager.DpiService.GetMonitorWorkArea(windowHandle);
		}
	}
}