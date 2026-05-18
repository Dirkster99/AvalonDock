using System.Linq;
using System.Windows;
using AvalonDock;
using AvalonDock.Layout;
using ToggleTestApp.ViewModels;

namespace ToggleTestApp
{
	public partial class MainWindow : Window
	{
		public MainWindow(MainViewModel viewModel)
		{
			DataContext = viewModel;
			InitializeComponent();
			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			// Open Terminal by default
			var terminal = FindAnchorable("Terminal");
			if (terminal != null)
			{
				dockManager.ToggleAnchorable(terminal, DockZone.BottomLeft);
			}
		}

		private LayoutAnchorable? FindAnchorable(string contentId)
		{
			foreach (var anc in dockManager.Layout.Descendents().OfType<LayoutAnchorable>())
			{
				if (anc.ContentId == contentId)
					return anc;
			}

			return null;
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
