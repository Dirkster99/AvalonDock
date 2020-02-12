/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace Microsoft.Windows.Shell
{
	using System;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Interop;
	using Standard;

	public static class SystemCommands
	{
		public static RoutedCommand CloseWindowCommand { get; }
		public static RoutedCommand MaximizeWindowCommand { get; }
		public static RoutedCommand MinimizeWindowCommand { get; }
		public static RoutedCommand RestoreWindowCommand { get; }
		public static RoutedCommand ShowSystemMenuCommand { get; }

		static SystemCommands()
		{
			CloseWindowCommand = new RoutedCommand(nameof(CloseWindow), typeof(SystemCommands));
			MaximizeWindowCommand = new RoutedCommand(nameof(MaximizeWindow), typeof(SystemCommands));
			MinimizeWindowCommand = new RoutedCommand(nameof(MinimizeWindow), typeof(SystemCommands));
			RestoreWindowCommand = new RoutedCommand(nameof(RestoreWindow), typeof(SystemCommands));
			ShowSystemMenuCommand = new RoutedCommand(nameof(ShowSystemMenu), typeof(SystemCommands));
		}

		private static void _PostSystemCommand(Window window, SC command)
		{
			var hWnd = new WindowInteropHelper(window).Handle;
			if (hWnd == IntPtr.Zero || !NativeMethods.IsWindow(hWnd)) return;
			NativeMethods.PostMessage(hWnd, WM.SYSCOMMAND, new IntPtr((int)command), IntPtr.Zero);
		}

		public static void CloseWindow(Window window)
		{
			Verify.IsNotNull(window, nameof(window));
			_PostSystemCommand(window, SC.CLOSE);
		}

		public static void MaximizeWindow(Window window)
		{
			Verify.IsNotNull(window, nameof(window));
			_PostSystemCommand(window, SC.MAXIMIZE);
		}

		public static void MinimizeWindow(Window window)
		{
			Verify.IsNotNull(window, nameof(window));
			_PostSystemCommand(window, SC.MINIMIZE);
		}

		public static void RestoreWindow(Window window)
		{
			Verify.IsNotNull(window, nameof(window));
			_PostSystemCommand(window, SC.RESTORE);
		}

		/// <summary>Display the system menu at a specified location.</summary>
		/// <param name="window"></param>
		/// <param name="screenLocation">The location to display the system menu, in logical screen coordinates.</param>
		public static void ShowSystemMenu(Window window, Point screenLocation)
		{
			Verify.IsNotNull(window, nameof(window));
			ShowSystemMenuPhysicalCoordinates(window, DpiHelper.LogicalPixelsToDevice(screenLocation));
		}

		internal static void ShowSystemMenuPhysicalCoordinates(Window window, Point physicalScreenLocation)
		{
			const uint TPM_RETURNCMD = 0x0100;
			const uint TPM_LEFTBUTTON = 0x0;

			Verify.IsNotNull(window, nameof(window));
			var hWnd = new WindowInteropHelper(window).Handle;
			if (hWnd == IntPtr.Zero || !NativeMethods.IsWindow(hWnd)) return;
			var hMenu = NativeMethods.GetSystemMenu(hWnd, false);
			var cmd = NativeMethods.TrackPopupMenuEx(hMenu, TPM_LEFTBUTTON | TPM_RETURNCMD, (int)physicalScreenLocation.X, (int)physicalScreenLocation.Y, hWnd, IntPtr.Zero);
			if (cmd != 0) NativeMethods.PostMessage(hWnd, WM.SYSCOMMAND, new IntPtr(cmd), IntPtr.Zero);
		}
	}
}
