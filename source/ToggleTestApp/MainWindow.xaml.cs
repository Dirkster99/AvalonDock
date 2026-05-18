using System.Windows;
using AvalonDock;
using AvalonDock.DependencyInjection;
using AvalonDock.Themes;
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

		private void OnThemeChanged(object sender, RoutedEventArgs e)
		{
			menuArcDark.IsChecked = sender == menuArcDark;
			menuArcLight.IsChecked = sender == menuArcLight;

			if (menuArcDark.IsChecked)
			{
				dockManager.Theme = new ArcDarkTheme();
				Background = new System.Windows.Media.SolidColorBrush(
					(System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#252729"));
			}
			else
			{
				dockManager.Theme = new ArcLightTheme();
				Background = new System.Windows.Media.SolidColorBrush(
					(System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F5F5F5"));
			}
		}

		private void OnExit(object sender, RoutedEventArgs e) => Close();
	}
}
