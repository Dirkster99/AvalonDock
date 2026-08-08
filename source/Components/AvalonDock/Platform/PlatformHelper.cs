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
		private sealed class NativeWindowHandle
		{
			internal NativeWindowHandle(IntPtr value) => Value = value;
			internal IntPtr Value { get; }
		}

		private static readonly ConditionalWeakTable<Window, NativeWindowHandle> NativeWindowHandles = new();
		private static readonly Dictionary<IntPtr, WeakReference<Window>> NativeWindowOwners = new();

		internal static bool CacheNativeWindowHandle(Window window)
		{
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
		}

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

		internal static void SetWindowPosition(Window window, double x, double y)
		{
			var windowHandle = GetNativeWindowHandle(window);
			if (windowHandle != IntPtr.Zero)
				PlatformManager.NativeWindowService.SetWindowPosition(windowHandle, x, y);
		}

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
		internal static IntPtr GetNativeWindowHandle(Window window)
		{
			if (window == null)
				return IntPtr.Zero;

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
