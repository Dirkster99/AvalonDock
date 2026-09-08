using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
#if LIBREWPF
using System.Windows.Media.ProGPU;
#endif

namespace TestApp
{
	/// <summary>
	/// Reports the X server's window stacking order so the DevFlow z-order verb can answer on Linux.
	/// <para>
	/// The Windows path walks <c>user32!GetWindow(GW_HWNDPREV)</c> and the macOS path indexes
	/// <c>NSApplication.orderedWindows</c>; neither has an equivalent under LibreWPF's portable
	/// backend on Linux. The <c>GetWindow</c> the app links against is the ProGPU Win32 compatibility
	/// shim, whose window table is entirely synthetic - it is populated by <c>CreateWindowEx</c> calls
	/// that the portable backend never makes, has no link to the real GLFW/X11 windows, and
	/// <c>GetWindow</c> itself is a stub returning 0. Worse, the handle a caller would pass it comes
	/// from <c>PortablePresentationSource</c> (a managed counter starting at 0x505750460000), which the
	/// shim has never seen. So the walk cannot be made to work from the Win32 side at all.
	/// </para>
	/// <para>
	/// The X server, on the other hand, knows the real answer. Each LibreWPF window is a genuine X11
	/// toplevel (GLFW on X11, or on XWayland when the session is Wayland), reachable through the
	/// Silk.NET native window, and EWMH-compliant window managers publish the full bottom-to-top
	/// stacking order of managed clients on the root window as <c>_NET_CLIENT_LIST_STACKING</c>.
	/// Indexing that list gives the same "higher number is closer to the front" convention the Win32
	/// walk produces, so <see cref="MainWindow"/> can compare the two platforms' results identically.
	/// </para>
	/// </summary>
	internal static class X11WindowStacking
	{
		private const string LibX11 = "libX11.so.6";

		/// <summary>XA_WINDOW - the predefined atom for a property holding window ids.</summary>
		private static readonly IntPtr XaWindow = new IntPtr(33);

		private static IntPtr s_display;
		private static IntPtr s_stackingAtom;
		private static bool s_unavailable;

		[DllImport(LibX11)]
		private static extern IntPtr XOpenDisplay(IntPtr display);

		[DllImport(LibX11)]
		private static extern IntPtr XDefaultRootWindow(IntPtr display);

		[DllImport(LibX11, CharSet = CharSet.Ansi)]
		private static extern IntPtr XInternAtom(IntPtr display, string atomName, bool onlyIfExists);

		[DllImport(LibX11)]
		private static extern int XGetWindowProperty(
			IntPtr display, IntPtr window, IntPtr property,
			IntPtr longOffset, IntPtr longLength, bool delete, IntPtr requestedType,
			out IntPtr actualType, out int actualFormat,
			out IntPtr itemCount, out IntPtr bytesAfter, out IntPtr data);

		[DllImport(LibX11)]
		private static extern int XFree(IntPtr data);

		/// <summary>
		/// True when an X display is reachable. False on a native-Wayland session (no XWayland) or
		/// when libX11 is absent, in which case every query below reports "not found".
		/// </summary>
		internal static bool IsAvailable => TryGetDisplay(out _);

		/// <summary>
		/// Resolves the real X11 window id backing a WPF window, or <see cref="IntPtr.Zero"/> when the
		/// window has no native surface yet (never shown, already closed) or the backend is not X11.
		/// </summary>
		internal static IntPtr TryGetWindowId(Window window)
		{
			if (window == null)
				return IntPtr.Zero;

#if !LIBREWPF
			// ProGPU types only exist in the LibreWPF build; this whole class is Linux-only anyway.
			return IntPtr.Zero;
#else
			try
			{
				if (!ProGpuWpfDiagnostics.TryGetWindowHost(window, out var host) ||
					host?.SilkWindow?.Native is not { } native)
					return IntPtr.Zero;

				// Silk.NET reports X11 as (Display, Window); Item2 is the XID.
				if (native.X11 is { Item2: var xid } && xid != UIntPtr.Zero)
					return (IntPtr)xid;
			}
			catch
			{
				// A window torn down between the host lookup and the native query has no id to report.
			}

			return IntPtr.Zero;
#endif
		}

		/// <summary>
		/// Returns the window manager's stacking order, bottom-most first, or an empty array when it
		/// cannot be read.
		/// </summary>
		internal static IntPtr[] GetStackingOrder()
		{
			if (!TryGetDisplay(out var display))
				return Array.Empty<IntPtr>();

			var data = IntPtr.Zero;
			try
			{
				var root = XDefaultRootWindow(display);
				var status = XGetWindowProperty(
					display, root, s_stackingAtom,
					IntPtr.Zero, new IntPtr(4096), false, XaWindow,
					out var actualType, out var actualFormat,
					out var itemCount, out _, out data);

				if (status != 0 /* Success */ || data == IntPtr.Zero || actualType != XaWindow || actualFormat != 32)
					return Array.Empty<IntPtr>();

				var count = (int)itemCount;
				var windows = new IntPtr[count];
				for (var i = 0; i < count; i++)
				{
					// A format-32 property is handed back as an array of C long, which is 8 bytes on
					// LP64 - not 4. Reading it with a 32-bit stride is the classic way to get garbage
					// out of XGetWindowProperty, so step by IntPtr.Size.
					windows[i] = Marshal.ReadIntPtr(data, i * IntPtr.Size);
				}

				return windows;
			}
			catch
			{
				return Array.Empty<IntPtr>();
			}
			finally
			{
				if (data != IntPtr.Zero)
					XFree(data);
			}
		}

		/// <summary>
		/// Reports where a window sits in the stacking order. Higher is closer to the front, matching
		/// the convention <c>MainWindow.TryGetWindowZOrder</c> produces from the Win32 walk.
		/// </summary>
		internal static bool TryGetStackIndex(IntPtr windowId, out int zOrder)
		{
			zOrder = int.MinValue;
			if (windowId == IntPtr.Zero)
				return false;

			var stacking = GetStackingOrder();
			for (var i = 0; i < stacking.Length; i++)
			{
				if (stacking[i] == windowId)
				{
					zOrder = i;
					return true;
				}
			}

			return false;
		}

		/// <summary>Describes the current stacking for diagnostics, including which entries are ours.</summary>
		internal static List<Dictionary<string, object>> DescribeStacking(IReadOnlyDictionary<IntPtr, string> knownWindows)
		{
			var described = new List<Dictionary<string, object>>();
			var stacking = GetStackingOrder();
			for (var i = 0; i < stacking.Length; i++)
			{
				described.Add(new Dictionary<string, object>
				{
					["index"] = i,
					["windowId"] = stacking[i].ToInt64(),
					["owner"] = knownWindows != null && knownWindows.TryGetValue(stacking[i], out var owner) ? owner : null,
				});
			}

			return described;
		}

		private static bool TryGetDisplay(out IntPtr display)
		{
			display = s_display;
			if (s_unavailable)
				return false;
			if (display != IntPtr.Zero)
				return true;

			try
			{
				// Cached: a drag-heavy test run queries this repeatedly.
				display = XOpenDisplay(IntPtr.Zero);
				if (display == IntPtr.Zero)
				{
					s_unavailable = true;
					return false;
				}

				s_display = display;
				s_stackingAtom = XInternAtom(display, "_NET_CLIENT_LIST_STACKING", true);
				if (s_stackingAtom == IntPtr.Zero)
				{
					// The window manager does not publish EWMH stacking (or none is running).
					s_unavailable = true;
					s_display = IntPtr.Zero;
					display = IntPtr.Zero;
					return false;
				}

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
}
