using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using AvalonDock;
using AvalonDock.DependencyInjection;
using AvalonDock.Themes;
using AvalonDock.Themes.VS;
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

				if (Enum.TryParse<DockLayoutPriority>(dockOptions.LayoutPriority, out var priority))
				{
					dockManager.LayoutPriority = priority;
				}
			}

			ContentRendered += (_, _) => UpdateTitleBarColor();
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
			menuVs2015Dark.IsChecked = sender == menuVs2015Dark;
			menuVs2015Light.IsChecked = sender == menuVs2015Light;
			menuVs2015Blue.IsChecked = sender == menuVs2015Blue;

			Theme theme;
			bool isDark;

			if (sender == menuVs2015Dark)
			{
				theme = new VS2015DarkTheme();
				isDark = true;
			}
			else if (sender == menuVs2015Light)
			{
				theme = new VS2015LightTheme();
				isDark = false;
			}
			else if (sender == menuVs2015Blue)
			{
				theme = new VS2015BlueTheme();
				isDark = true;
			}
			else if (sender == menuArcLight)
			{
				theme = new ArcLightTheme();
				isDark = false;
			}
			else
			{
				theme = new ArcDarkTheme();
				isDark = true;
			}

			dockManager.Theme = theme;
			SetAppThemeResources(isDark);
			UpdateThemeColors();
		}

		private void SetAppThemeResources(bool isDark)
		{
			if (isDark)
			{
				Resources["AppPanelBg"] = Brush("#252526");
				Resources["AppEditorBg"] = Brush("#1E1E1E");
				Resources["AppInputBg"] = Brush("#3C3C3C");
				Resources["AppInputBarBg"] = Brush("#2D2D2D");
				Resources["AppText"] = Brush("#CCCCCC");
				Resources["AppSubText"] = Brush("#808080");
				Resources["AppDimText"] = Brush("#555555");
				Resources["AppEditorText"] = Brush("#D4D4D4");
				Resources["AppLineNumbers"] = Brush("#858585");
				Resources["AppScrollbarBg"] = Brush("#2B2B2B");
				Resources["AppSelection"] = Brush("#094771");
			}
			else
			{
				Resources["AppPanelBg"] = Brush("#F5F5F5");
				Resources["AppEditorBg"] = Brush("#FFFFFF");
				Resources["AppInputBg"] = Brush("#FFFFFF");
				Resources["AppInputBarBg"] = Brush("#E8E8E8");
				Resources["AppText"] = Brush("#1E1E1E");
				Resources["AppSubText"] = Brush("#616161");
				Resources["AppDimText"] = Brush("#999999");
				Resources["AppEditorText"] = Brush("#1E1E1E");
				Resources["AppLineNumbers"] = Brush("#858585");
				Resources["AppScrollbarBg"] = Brush("#E0E0E0");
				Resources["AppSelection"] = Brush("#B4D8FD");
			}
		}

		private void UpdateThemeColors()
		{
			Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
			{
				bool isDark = IsDarkBrush(dockManager.Background);
				var foreground = isDark ? Brushes.White : Brushes.Black;
				Foreground = foreground;

				UpdateTitleBarColor();
			}));
		}

		[DllImport("dwmapi.dll", PreserveSig = true)]
		private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

		private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
		private const int DWMWA_CAPTION_COLOR = 35;

		private void UpdateTitleBarColor()
		{
			var hwnd = new WindowInteropHelper(this).Handle;
			if (hwnd == IntPtr.Zero)
				return;

			bool isDark = IsDarkBrush(dockManager.Background);
			int darkMode = isDark ? 1 : 0;
			DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

			if (dockManager.Background is SolidColorBrush scb)
			{
				var c = scb.Color;
				int colorRef = c.R | (c.G << 8) | (c.B << 16);
				DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref colorRef, sizeof(int));
			}
		}

		private static bool IsDarkBrush(Brush brush)
		{
			if (brush is SolidColorBrush scb)
			{
				var c = scb.Color;
				var luminance = (0.299 * c.R) + (0.587 * c.G) + (0.114 * c.B);
				return luminance < 128;
			}

			return true;
		}

		private static SolidColorBrush Brush(string hex)
		{
			var color = (Color)ColorConverter.ConvertFromString(hex);
			var brush = new SolidColorBrush(color);
			brush.Freeze();
			return brush;
		}

		private void OnExit(object sender, RoutedEventArgs e) => Close();
	}
}