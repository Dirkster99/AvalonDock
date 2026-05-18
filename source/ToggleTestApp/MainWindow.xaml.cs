using System.Windows;
using AvalonDock;
using AvalonDock.DependencyInjection;
using ToggleTestApp.ViewModels;

namespace ToggleTestApp
{
	public partial class MainWindow : Window
	{
		public MainWindow(MainViewModel viewModel, ToggleDockOptions? dockOptions = null)
		{
			DataContext = viewModel;
			InitializeComponent();

			if (dockOptions != null)
			{
				dockManager.ButtonSize = dockOptions.ButtonSize;
				dockManager.DefaultDockWidth = dockOptions.DefaultDockWidth;
				dockManager.DefaultDockHeight = dockOptions.DefaultDockHeight;
				dockManager.ShowHeaderMinimizeButton = dockOptions.ShowHeaderMinimizeButton;
				dockManager.ShowHeaderOptionsButton = dockOptions.ShowHeaderOptionsButton;

				if (System.Enum.TryParse<DockLayoutPriority>(dockOptions.LayoutPriority, out var priority))
				{
					dockManager.LayoutPriority = priority;
				}
			}
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

		private void OnExit(object sender, RoutedEventArgs e) => Close();
	}
}
