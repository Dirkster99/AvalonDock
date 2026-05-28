using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ToggleTestApp.ViewModels;

/// <summary>
/// Provides sidebar toggle button icons as Viewbox instances.
/// Each icon binds its strokes/fills to the inherited TextElement.Foreground
/// so icons automatically reflect the active/inactive color from the button's triggers.
/// </summary>
public static class ToolboxIcons
{
	public static object Explorer => CreateExplorerIcon();
	public static object Terminal => CreateTerminalIcon();
	public static object Search => CreateSearchIcon();
	public static object Git => CreateGitIcon();
	public static object Problems => CreateProblemsIcon();

	private static Binding ForegroundBinding() => new Binding
	{
		Path = new PropertyPath(TextElement.ForegroundProperty),
		RelativeSource = new RelativeSource(RelativeSourceMode.Self)
	};

	private static Viewbox CreateExplorerIcon()
	{
		var canvas = new Canvas { Width = 16, Height = 16 };

		var folder = new Path
		{
			Data = Geometry.Parse(
				"M1.5,1 L6,1 L7.5,3 L14.5,3 C15.3,3 15.5,3.5 15.5,4 L15.5,13 C15.5,13.5 15,14 14.5,14 L1.5,14 C1,14 0.5,13.5 0.5,13 L0.5,2 C0.5,1.5 1,1 1.5,1 Z"),
			StrokeThickness = 0.8,
			Fill = Brushes.Transparent
		};
		folder.SetBinding(Shape.StrokeProperty, ForegroundBinding());

		var line = new Line { X1 = 0.5, Y1 = 5.5, X2 = 15.5, Y2 = 5.5, StrokeThickness = 0.6 };
		line.SetBinding(Shape.StrokeProperty, ForegroundBinding());

		canvas.Children.Add(folder);
		canvas.Children.Add(line);
		return new Viewbox { Width = 16, Height = 16, Child = canvas };
	}

	private static Viewbox CreateTerminalIcon()
	{
		var canvas = new Canvas { Width = 16, Height = 16 };

		var rect = new Rectangle
		{
			Width = 15, Height = 13,
			RadiusX = 1.5, RadiusY = 1.5,
			StrokeThickness = 0.8,
			Fill = Brushes.Transparent
		};
		Canvas.SetLeft(rect, 0.5);
		Canvas.SetTop(rect, 1.5);
		rect.SetBinding(Shape.StrokeProperty, ForegroundBinding());

		var prompt = new Path
		{
			Data = Geometry.Parse("M4,6 L7,8.5 L4,11"),
			StrokeThickness = 1.5,
			Fill = Brushes.Transparent
		};
		prompt.SetBinding(Shape.StrokeProperty, ForegroundBinding());

		var cursor = new Rectangle { Width = 4, Height = 1.2 };
		Canvas.SetLeft(cursor, 8.5);
		Canvas.SetTop(cursor, 10.5);
		cursor.SetBinding(Shape.FillProperty, ForegroundBinding());

		canvas.Children.Add(rect);
		canvas.Children.Add(prompt);
		canvas.Children.Add(cursor);
		return new Viewbox { Width = 16, Height = 16, Child = canvas };
	}

	private static Viewbox CreateSearchIcon()
	{
		var canvas = new Canvas { Width = 16, Height = 16 };

		var circle = new Ellipse { Width = 9, Height = 9, StrokeThickness = 1.2, Fill = Brushes.Transparent };
		Canvas.SetLeft(circle, 2);
		Canvas.SetTop(circle, 2);
		circle.SetBinding(Shape.StrokeProperty, ForegroundBinding());

		var handle = new Line { X1 = 10, Y1 = 10, X2 = 14, Y2 = 14, StrokeThickness = 1.5 };
		handle.SetBinding(Shape.StrokeProperty, ForegroundBinding());

		canvas.Children.Add(circle);
		canvas.Children.Add(handle);
		return new Viewbox { Width = 16, Height = 16, Child = canvas };
	}

	private static Viewbox CreateGitIcon()
	{
		var canvas = new Canvas { Width = 16, Height = 16 };

		var c1 = new Ellipse { Width = 3.5, Height = 3.5, StrokeThickness = 1, Fill = Brushes.Transparent };
		Canvas.SetLeft(c1, 2.5);
		Canvas.SetTop(c1, 2);
		c1.SetBinding(Shape.StrokeProperty, ForegroundBinding());

		var c2 = new Ellipse { Width = 3.5, Height = 3.5, StrokeThickness = 1, Fill = Brushes.Transparent };
		Canvas.SetLeft(c2, 9);
		Canvas.SetTop(c2, 2);
		c2.SetBinding(Shape.StrokeProperty, ForegroundBinding());

		var c3 = new Ellipse { Width = 3.5, Height = 3.5, StrokeThickness = 1, Fill = Brushes.Transparent };
		Canvas.SetLeft(c3, 2.5);
		Canvas.SetTop(c3, 10.5);
		c3.SetBinding(Shape.StrokeProperty, ForegroundBinding());

		var stem = new Line { X1 = 4.25, Y1 = 5.5, X2 = 4.25, Y2 = 10.5, StrokeThickness = 1 };
		stem.SetBinding(Shape.StrokeProperty, ForegroundBinding());

		var branch = new Path
		{
			Data = Geometry.Parse("M10.75,5.5 C10.75,8.5 4.25,8.5 4.25,10.5"),
			StrokeThickness = 1,
			Fill = Brushes.Transparent
		};
		branch.SetBinding(Shape.StrokeProperty, ForegroundBinding());

		canvas.Children.Add(c1);
		canvas.Children.Add(c2);
		canvas.Children.Add(c3);
		canvas.Children.Add(stem);
		canvas.Children.Add(branch);
		return new Viewbox { Width = 16, Height = 16, Child = canvas };
	}

	private static Viewbox CreateProblemsIcon()
	{
		var canvas = new Canvas { Width = 16, Height = 16 };

		var triangle = new Path
		{
			Data = Geometry.Parse("M8,1.5 L15,13.5 L1,13.5 Z"),
			StrokeThickness = 1,
			Fill = Brushes.Transparent
		};
		triangle.SetBinding(Shape.StrokeProperty, ForegroundBinding());

		var excl = new Line { X1 = 8, Y1 = 6, X2 = 8, Y2 = 10, StrokeThickness = 1.2 };
		excl.SetBinding(Shape.StrokeProperty, ForegroundBinding());

		var dot = new Ellipse { Width = 1.6, Height = 1.6 };
		Canvas.SetLeft(dot, 7.2);
		Canvas.SetTop(dot, 11);
		dot.SetBinding(Shape.FillProperty, ForegroundBinding());

		canvas.Children.Add(triangle);
		canvas.Children.Add(excl);
		canvas.Children.Add(dot);
		return new Viewbox { Width = 16, Height = 16, Child = canvas };
	}
}