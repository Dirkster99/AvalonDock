using System.Windows;

namespace ToggleTestApp
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void OnDumpToConsole(object sender, RoutedEventArgs e)
		{
#if DEBUG
			dockManager.Layout.ConsoleDump(0);
#endif
		}
	}
}
