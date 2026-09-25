using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace AvalonDock.Controls
{
	internal sealed class LayoutGridResizerGhostAdorner : Adorner
	{
		private readonly Brush _brush;
		private readonly double _opacity;
		private readonly Size _ghostSize;

		internal LayoutGridResizerGhostAdorner(UIElement adornedElement, Brush brush, double opacity, Size ghostSize, Point position)
			: base(adornedElement)
		{
			_brush = brush;
			_opacity = opacity;
			_ghostSize = ghostSize;
			Position = position;
			IsHitTestVisible = false;
		}

		internal Point Position { get; set; }

		protected override void OnRender(DrawingContext drawingContext)
		{
			drawingContext.PushOpacity(_opacity);
			drawingContext.DrawRectangle(_brush, null, new Rect(Position, _ghostSize));
			drawingContext.Pop();
		}
	}
}
