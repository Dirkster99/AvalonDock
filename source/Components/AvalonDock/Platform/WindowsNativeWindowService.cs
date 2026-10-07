using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace AvalonDock.Platform
{
	/// <summary>
	/// Windows implementation of INativeWindowService.
	/// Uses Win32 APIs for window operations.
	/// </summary>
	internal class WindowsNativeWindowService : INativeWindowService
	{
		public (double X, double Y) GetWindowPosition(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero) return (0, 0);

			GetWindowRect(windowHandle, out var rect);
			return (rect.Left, rect.Top);
		}

		public void SetWindowPosition(IntPtr windowHandle, double x, double y)
		{
			if (windowHandle == IntPtr.Zero) return;

			SetWindowPos(windowHandle, IntPtr.Zero, (int)x, (int)y, 0, 0,
				SetWindowPosFlags.IgnoreResize | SetWindowPosFlags.NoActivate);
		}

		public (double X, double Y, double Width, double Height) GetWindowFrame(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero) return (0, 0, 0, 0);

			GetWindowRect(windowHandle, out var rect);
			return (rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
		}

		public void BringToFront(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero) return;

			BringWindowToTop(windowHandle);
		}

		public void CloseWindow(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero) return;

			DestroyWindow(windowHandle);
		}

		public void DisableWindowTabbing(IntPtr windowHandle)
		{
			// No-op on Windows - tabbing is not a concept in Win32
		}

		public void SetWindowAlpha(IntPtr windowHandle, double alpha)
		{
			if (windowHandle == IntPtr.Zero) return;

			int extendedStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
			SetWindowLong(windowHandle, GWL_EXSTYLE, extendedStyle | WS_EX_LAYERED);
			SetLayeredWindowAttributes(windowHandle, 0, (byte)(alpha * 255), LWA_ALPHA);
		}

		public void SetWindowLevel(IntPtr windowHandle, int level)
		{
			if (windowHandle == IntPtr.Zero) return;

			IntPtr hWndInsertAfter;
			if (level > 0)
				hWndInsertAfter = HWND_TOPMOST;
			else if (level < 0)
				hWndInsertAfter = HWND_BOTTOM;
			else
				hWndInsertAfter = HWND_TOP;

			SetWindowPos(windowHandle, hWndInsertAfter, 0, 0, 0, 0,
				SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize |
				SetWindowPosFlags.DoNotActivate | SetWindowPosFlags.DoNotChangeOwnerZOrder);
		}

		public (double X, double Y, double Width, double Height) GetContentViewFrame(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero) return (0, 0, 0, 0);

			GetClientRect(windowHandle, out var rect);
			return (0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);
		}

		public (double X, double Y) GetWindowContentOrigin(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero) return (0, 0);

			var point = new Point(0, 0);
			ClientToScreen(windowHandle, ref point);
			return (point.X, point.Y);
		}

		#region Native Methods

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
			int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool BringWindowToTop(IntPtr hWnd);

		[DllImport("user32.dll", EntryPoint = "DestroyWindow")]
		private static extern bool DestroyWindow(IntPtr hwnd);

		[DllImport("user32.dll")]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey,
			byte bAlpha, uint dwFlags);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[Flags]
		private enum SetWindowPosFlags : uint
		{
			IgnoreMove = 0x0002,
			IgnoreResize = 0x0001,
			DoNotActivate = 0x0010,
			DoNotChangeOwnerZOrder = 0x0200,
			NoActivate = 0x0010
		}

		private const int GWL_EXSTYLE = -20;
		private const int WS_EX_LAYERED = 0x00080000;
		private const uint LWA_ALPHA = 0x00000002;

		private static readonly IntPtr HWND_TOP = IntPtr.Zero;
		private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
		private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

		#endregion
	}
}