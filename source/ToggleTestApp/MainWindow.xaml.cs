using System.Linq;
using System.Windows;
using AvalonDock;
using AvalonDock.DependencyInjection;
using AvalonDock.Layout;
using ToggleTestApp.ViewModels;

namespace ToggleTestApp
{
	public partial class MainWindow : Window
	{
		private readonly ToggleDockOptions? _dockOptions;

		public MainWindow(MainViewModel viewModel, ToggleDockOptions? dockOptions = null)
		{
			_dockOptions = dockOptions;
			DataContext = viewModel;
			InitializeComponent();
			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			// Apply DI-configured options
			if (_dockOptions != null)
			{
				dockManager.ButtonSize = _dockOptions.ButtonSize;
				dockManager.DefaultDockWidth = _dockOptions.DefaultDockWidth;
				dockManager.DefaultDockHeight = _dockOptions.DefaultDockHeight;
				dockManager.ShowHeaderMinimizeButton = _dockOptions.ShowHeaderMinimizeButton;
				dockManager.ShowHeaderOptionsButton = _dockOptions.ShowHeaderOptionsButton;

				if (System.Enum.TryParse<DockLayoutPriority>(_dockOptions.LayoutPriority, out var priority))
				{
					dockManager.LayoutPriority = priority;
				}
			}

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
