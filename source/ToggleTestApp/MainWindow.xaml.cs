using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace ToggleTestApp
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			// Assign vector icons to toggle buttons after the manager has set them up
			AssignIconToButton("ToolBox 1", CreateWrenchIcon());
			AssignIconToButton("ToolBox 2", CreateWrenchIcon());
			AssignIconToButton("Properties", CreateSlidersIcon());
			AssignIconToButton("Output", CreateTerminalIcon());
		}

		private void AssignIconToButton(string title, UIElement icon)
		{
			// Walk the visual tree to find ToggleDockButtons
			foreach (var btn in FindVisualChildren<ToggleDockButton>(dockManager))
			{
				if (btn.Anchorable?.Title == title)
				{
					btn.IconContent = icon;
					break;
				}
			}
		}

		private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
		{
			if (parent == null) yield break;
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				if (child is T t) yield return t;
				foreach (var grandChild in FindVisualChildren<T>(child))
					yield return grandChild;
			}
		}

		private static UIElement CreateWrenchIcon()
		{
			var path = new Path
			{
				Data = Geometry.Parse("M14.7,6.3l-1.6-1.6c-0.2-0.2-0.5-0.2-0.7,0L8.3,8.8L7.2,7.7l4.1-4.1c0.2-0.2,0.2-0.5,0-0.7L9.7,1.3C9.1,0.7,8.3,0.5,7.5,0.7L5.9,2.3L7.5,3.9L6.1,5.3L4.5,3.7L2.3,5.9C2.1,6.5,2.3,7.3,2.9,7.9l0,0l-2.6,2.6c-0.4,0.4-0.4,1,0,1.4l1.8,1.8c0.4,0.4,1,0.4,1.4,0l2.6-2.6c0.6,0.6,1.4,0.8,2,0.6l1.6-1.6L8.1,8.5l1.4-1.4l1.6,1.6l4.1-4.1C15.3,4.5,14.7,6.3,14.7,6.3z"),
				Fill = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
				Stretch = Stretch.Uniform,
				Width = 14,
				Height = 14
			};
			return path;
		}

		private static UIElement CreateSlidersIcon()
		{
			var canvas = new Canvas { Width = 16, Height = 16 };
			var accent = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
			var gray = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));

			canvas.Children.Add(new Rectangle { Width = 14, Height = 2, RadiusX = 1, RadiusY = 1, Fill = gray });
			Canvas.SetLeft(canvas.Children[0], 1); Canvas.SetTop(canvas.Children[0], 2);
			canvas.Children.Add(new Ellipse { Width = 4, Height = 4, Fill = accent });
			Canvas.SetLeft(canvas.Children[1], 9); Canvas.SetTop(canvas.Children[1], 1);
			canvas.Children.Add(new Rectangle { Width = 14, Height = 2, RadiusX = 1, RadiusY = 1, Fill = gray });
			Canvas.SetLeft(canvas.Children[2], 1); Canvas.SetTop(canvas.Children[2], 7);
			canvas.Children.Add(new Ellipse { Width = 4, Height = 4, Fill = accent });
			Canvas.SetLeft(canvas.Children[3], 3); Canvas.SetTop(canvas.Children[3], 6);
			canvas.Children.Add(new Rectangle { Width = 14, Height = 2, RadiusX = 1, RadiusY = 1, Fill = gray });
			Canvas.SetLeft(canvas.Children[4], 1); Canvas.SetTop(canvas.Children[4], 12);
			canvas.Children.Add(new Ellipse { Width = 4, Height = 4, Fill = accent });
			Canvas.SetLeft(canvas.Children[5], 7); Canvas.SetTop(canvas.Children[5], 11);

			var vb = new Viewbox { Width = 14, Height = 14, Child = canvas };
			return vb;
		}

		private static UIElement CreateTerminalIcon()
		{
			var canvas = new Canvas { Width = 16, Height = 16 };
			var gray = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
			var accent = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));

			canvas.Children.Add(new Rectangle { Width = 14, Height = 12, Stroke = gray, StrokeThickness = 1.2, RadiusX = 1, RadiusY = 1, Fill = Brushes.Transparent });
			Canvas.SetLeft(canvas.Children[0], 1); Canvas.SetTop(canvas.Children[0], 2);
			canvas.Children.Add(new Path { Data = Geometry.Parse("M4,7 L7,9.5 L4,12"), Stroke = accent, StrokeThickness = 1.5, Fill = Brushes.Transparent });
			canvas.Children.Add(new Rectangle { Width = 4, Height = 1.2, Fill = gray });
			Canvas.SetLeft(canvas.Children[2], 8); Canvas.SetTop(canvas.Children[2], 11);

			var vb = new Viewbox { Width = 14, Height = 14, Child = canvas };
			return vb;
		}

		private void OnDumpToConsole(object sender, RoutedEventArgs e)
		{
#if DEBUG
			dockManager.Layout.ConsoleDump(0);
#endif
		}
	}
}
