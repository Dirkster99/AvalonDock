using System.Windows;
using AvalonDock;
using ToggleTestApp.ViewModels;

namespace ToggleTestApp
{
	public partial class MainWindow : Window
	{
		public MainWindow(MainViewModel viewModel)
		{
			DataContext = viewModel;
			InitializeComponent();
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

		private void OnExit(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
