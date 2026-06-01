namespace Microsoft.Windows.Shell
{
	using System;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Interop;
	using Standard;

	/// <summary>
	/// Provides helper members for system Commands.
	/// </summary>
	public static class SystemCommands
	{
		/// <summary>
		/// Gets the close Window Command.
		/// </summary>
		public static RoutedCommand CloseWindowCommand { get; }

		/// <summary>
		/// Gets the maximize Window Command.
		/// </summary>
		public static RoutedCommand MaximizeWindowCommand { get; }

		/// <summary>
		/// Gets the minimize Window Command.
		/// </summary>
		public static RoutedCommand MinimizeWindowCommand { get; }

		/// <summary>
		/// Gets the restore Window Command.
		/// </summary>
		public static RoutedCommand RestoreWindowCommand { get; }

		/// <summary>
		/// Gets the show System Menu Command.
		/// </summary>
		public static RoutedCommand ShowSystemMenuCommand { get; }

		/// <summary>
		/// Initializes static members of the <see cref="SystemCommands"/> class.
		/// </summary>
		static SystemCommands()
		{
			CloseWindowCommand = new RoutedCommand(nameof(CloseWindow), typeof(SystemCommands));
			MaximizeWindowCommand = new RoutedCommand(nameof(MaximizeWindow), typeof(SystemCommands));
			MinimizeWindowCommand = new RoutedCommand(nameof(MinimizeWindow), typeof(SystemCommands));
			RestoreWindowCommand = new RoutedCommand(nameof(RestoreWindow), typeof(SystemCommands));
			ShowSystemMenuCommand = new RoutedCommand(nameof(ShowSystemMenu), typeof(SystemCommands));
		}

		/// <summary>
		/// Executes the post System Command operation.
		/// </summary>
		/// <param name="window">The window.</param>
		/// <param name="command">The command.</param>
		private static void _PostSystemCommand(Window window, SC command)
		{
			var hWnd = new WindowInteropHelper(window).Handle;
			if (hWnd == IntPtr.Zero || !NativeMethods.IsWindow(hWnd)) return;
			NativeMethods.PostMessage(hWnd, WM.SYSCOMMAND, new IntPtr((int)command), IntPtr.Zero);
		}

		/// <summary>
		/// Executes the close Window operation.
		/// </summary>
		/// <param name="window">The window.</param>
		public static void CloseWindow(Window window)
		{
			Verify.IsNotNull(window, nameof(window));
			_PostSystemCommand(window, SC.CLOSE);
		}

		/// <summary>
		/// Executes the maximize Window operation.
		/// </summary>
		/// <param name="window">The window.</param>
		public static void MaximizeWindow(Window window)
		{
			Verify.IsNotNull(window, nameof(window));
			_PostSystemCommand(window, SC.MAXIMIZE);
		}

		/// <summary>
		/// Executes the minimize Window operation.
		/// </summary>
		/// <param name="window">The window.</param>
		public static void MinimizeWindow(Window window)
		{
			Verify.IsNotNull(window, nameof(window));
			_PostSystemCommand(window, SC.MINIMIZE);
		}

		/// <summary>
		/// Executes the restore Window operation.
		/// </summary>
		/// <param name="window">The window.</param>
		public static void RestoreWindow(Window window)
		{
			Verify.IsNotNull(window, nameof(window));
			_PostSystemCommand(window, SC.RESTORE);
		}

		/// <summary>
		/// Executes the show System Menu operation.
		/// </summary>
		/// <param name="window">The window.</param>
		/// <param name="screenLocation">The screen Location.</param>
		public static void ShowSystemMenu(Window window, Point screenLocation)
		{
			Verify.IsNotNull(window, nameof(window));
			ShowSystemMenuPhysicalCoordinates(window, DpiHelper.LogicalPixelsToDevice(screenLocation));
		}

		/// <summary>
		/// Executes the show System Menu Physical Coordinates operation.
		/// </summary>
		/// <param name="window">The window.</param>
		/// <param name="physicalScreenLocation">The physical Screen Location.</param>
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