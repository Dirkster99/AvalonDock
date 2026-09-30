using System.Windows;
using System.Windows.Input;

namespace Microsoft.Windows.Shell
{
	public static class SystemCommands
	{
		public static RoutedCommand CloseWindowCommand => System.Windows.SystemCommands.CloseWindowCommand;
		public static RoutedCommand MaximizeWindowCommand => System.Windows.SystemCommands.MaximizeWindowCommand;
		public static RoutedCommand MinimizeWindowCommand => System.Windows.SystemCommands.MinimizeWindowCommand;
		public static RoutedCommand RestoreWindowCommand => System.Windows.SystemCommands.RestoreWindowCommand;
		public static RoutedCommand ShowSystemMenuCommand => System.Windows.SystemCommands.ShowSystemMenuCommand;

		public static void CloseWindow(Window window) => System.Windows.SystemCommands.CloseWindow(window);
		public static void MaximizeWindow(Window window) => System.Windows.SystemCommands.MaximizeWindow(window);
		public static void MinimizeWindow(Window window) => System.Windows.SystemCommands.MinimizeWindow(window);
		public static void RestoreWindow(Window window) => System.Windows.SystemCommands.RestoreWindow(window);
		public static void ShowSystemMenu(Window window, Point screenLocation) =>
			System.Windows.SystemCommands.ShowSystemMenu(window, screenLocation);
	}
}
