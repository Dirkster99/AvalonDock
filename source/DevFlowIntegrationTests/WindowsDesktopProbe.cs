using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace AvalonDock.DevFlowIntegrationTests
{
	/// <summary>
	/// Describes which native window owns a screen point on Windows. An app-level hit test only sees the
	/// app's own visual tree, so when a real click "does not reach" WPF it cannot tell whether another
	/// window (another process, or a leftover window of the app) is the one Windows delivers it to.
	/// </summary>
	internal static class WindowsDesktopProbe
	{
		public static string DescribePoint(double screenX, double screenY)
		{
			if (!OperatingSystem.IsWindows())
				return "n/a";
			try
			{
				var atPoint = WindowFromPoint(new NativePoint { X = (int)Math.Round(screenX), Y = (int)Math.Round(screenY) });
				var root = atPoint == IntPtr.Zero ? IntPtr.Zero : GetAncestor(atPoint, GaRoot);
				return $"atPoint={Describe(atPoint)}; root={Describe(root)}; foreground={Describe(GetForegroundWindow())}";
			}
			catch (Exception ex)
			{
				return $"probe failed: {ex.GetType().Name}: {ex.Message}";
			}
		}

		private static string Describe(IntPtr hwnd)
		{
			if (hwnd == IntPtr.Zero)
				return "none";
			var className = new StringBuilder(256);
			_ = GetClassName(hwnd, className, className.Capacity);
			var title = new StringBuilder(256);
			_ = GetWindowText(hwnd, title, title.Capacity);
			_ = GetWindowThreadProcessId(hwnd, out var processId);
			string process;
			try { process = Process.GetProcessById((int)processId).ProcessName; }
			catch (ArgumentException) { process = "?"; }
			_ = GetWindowRect(hwnd, out var rect);
			return $"0x{hwnd.ToInt64():X} {process}({processId}) class='{className}' title='{title}' rect={rect.Left},{rect.Top} {rect.Right - rect.Left}x{rect.Bottom - rect.Top}" +
				(processId == (uint)Environment.ProcessId ? " (test process)" : string.Empty);
		}

		private const uint GaRoot = 2;

		[StructLayout(LayoutKind.Sequential)]
		private struct NativePoint { public int X; public int Y; }

		[StructLayout(LayoutKind.Sequential)]
		private struct NativeRect { public int Left; public int Top; public int Right; public int Bottom; }

		[DllImport("user32.dll")] private static extern IntPtr WindowFromPoint(NativePoint point);
		[DllImport("user32.dll")] private static extern IntPtr GetAncestor(IntPtr hwnd, uint flags);
		[DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
		[DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(IntPtr hwnd, StringBuilder name, int max);
		[DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int max);
		[DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint processId);
		[DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out NativeRect rect);
	}
}
