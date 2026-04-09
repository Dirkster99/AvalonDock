using System.Windows;
using AvalonDock;

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

		private void OnLayoutPriorityChanged(object sender, RoutedEventArgs e)
		{
			menuBottomFullWidth.IsChecked = sender == menuBottomFullWidth;
			menuSidesFullHeight.IsChecked = sender == menuSidesFullHeight;
			menuDefaultPriority.IsChecked = sender == menuDefaultPriority;

			if (menuBottomFullWidth.IsChecked)
				dockManager.LayoutPriority = DockLayoutPriority.BottomFullWidth;
			else if (menuSidesFullHeight.IsChecked)
				dockManager.LayoutPriority = DockLayoutPriority.SidesFullHeight;
			else
				dockManager.LayoutPriority = DockLayoutPriority.Default;
		}
	}
}
