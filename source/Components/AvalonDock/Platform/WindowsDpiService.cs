using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace AvalonDock.Platform
{
	/// <summary>
	/// Windows implementation of IDpiService.
	/// Uses Win32 APIs for DPI operations.
	/// </summary>
	internal class WindowsDpiService : IDpiService
	{
		public double GetPrimaryMonitorDpi()
		{
			var hdc = GetDC(IntPtr.Zero);
			if (hdc == IntPtr.Zero) return 1.0;

			try
			{
				int dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
				int dpiY = GetDeviceCaps(hdc, LOGPIXELSY);
				return dpiX / 96.0; // 96 DPI is the base (100%)
			}
			finally
			{
				ReleaseDC(IntPtr.Zero, hdc);
			}
		}

		public double GetMonitorDpi(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero) return GetPrimaryMonitorDpi();

			var hdc = GetDC(windowHandle);
			if (hdc == IntPtr.Zero) return GetPrimaryMonitorDpi();

			try
			{
				int dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
				return dpiX / 96.0;
			}
			finally
			{
				ReleaseDC(windowHandle, hdc);
			}
		}

		public Rect GetPrimaryMonitorWorkArea()
		{
			var monitorInfo = new MONITORINFO();
			monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

			var hMonitor = MonitorFromWindow(IntPtr.Zero, MONITOR_DEFAULTTONEAREST);
			if (GetMonitorInfo(hMonitor, ref monitorInfo))
			{
				var workArea = monitorInfo.rcWork;
				return new Rect(workArea.Left, workArea.Top,
					workArea.Right - workArea.Left, workArea.Bottom - workArea.Top);
			}

			return new Rect(0, 0, 1920, 1080); // Fallback
		}

		public Rect GetMonitorWorkArea(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero) return GetPrimaryMonitorWorkArea();

			var monitorInfo = new MONITORINFO();
			monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

			var hMonitor = MonitorFromWindow(windowHandle, MONITOR_DEFAULTTONEAREST);
			if (GetMonitorInfo(hMonitor, ref monitorInfo))
			{
				var workArea = monitorInfo.rcWork;
				return new Rect(workArea.Left, workArea.Top,
					workArea.Right - workArea.Left, workArea.Bottom - workArea.Top);
			}

			return GetPrimaryMonitorWorkArea();
		}

		#region Native Methods

		[DllImport("user32.dll")]
		private static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport("gdi32.dll")]
		private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

		[DllImport("user32.dll")]
		private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MONITORINFO
		{
			public int cbSize;
			public RECT rcMonitor;
			public RECT rcWork;
			public uint dwFlags;
		}

		private const int LOGPIXELSX = 88;
		private const int LOGPIXELSY = 90;
		private const uint MONITOR_DEFAULTTONEAREST = 2;

		#endregion
	}
}