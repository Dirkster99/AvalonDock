using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace AvalonDock.Platform
{
	/// <summary>
	/// Windows implementation of ICursorService.
	/// Uses Win32 APIs for cursor operations.
	/// </summary>
	internal class WindowsCursorService : ICursorService
	{
		public (double X, double Y) GetCursorPosition()
		{
			if (GetCursorPos(out var point))
				return (point.X, point.Y);
			return (0, 0);
		}

		public bool IsLeftButtonDown()
		{
			return (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;
		}

		public (double X, double Y) GetCursorLocationQuartz()
		{
			// On Windows, Quartz coordinates are the same as screen coordinates
			return GetCursorPosition();
		}

		public (double X, double Y) GetCursorLocationCocoa()
		{
			// On Windows, return screen coordinates (no Cocoa conversion needed)
			return GetCursorPosition();
		}

		#region Native Methods

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetCursorPos(out POINT lpPoint);

		[DllImport("user32.dll")]
		private static extern short GetAsyncKeyState(int vKey);

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT
		{
			public int X;
			public int Y;
		}

		private const int VK_LBUTTON = 0x01;

		#endregion
	}
}