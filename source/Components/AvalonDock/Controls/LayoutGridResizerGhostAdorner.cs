using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace AvalonDock.Controls
{
	/// <summary>Draws the semi-transparent ghost of a grid resizer while it is dragged.</summary>
	internal sealed class LayoutGridResizerGhostAdorner : Adorner
	{
		private readonly Brush _brush;
		private readonly double _opacity;
		private readonly Size _ghostSize;

		/// <summary>Initializes a new instance of the <see cref="LayoutGridResizerGhostAdorner"/> class.</summary>
		/// <param name="adornedElement">The element the ghost is drawn over.</param>
		/// <param name="brush">The brush the ghost is filled with.</param>
		/// <param name="opacity">The opacity of the ghost.</param>
		/// <param name="ghostSize">The size of the ghost.</param>
		/// <param name="position">The initial position of the ghost.</param>
		internal LayoutGridResizerGhostAdorner(UIElement adornedElement, Brush brush, double opacity, Size ghostSize, Point position)
			: base(adornedElement)
		{
			_brush = brush;
			_opacity = opacity;
			_ghostSize = ghostSize;
			Position = position;
			IsHitTestVisible = false;
		}

		/// <summary>Gets or sets the position of the ghost.</summary>
		internal Point Position { get; set; }

		/// <inheritdoc/>
		protected override void OnRender(DrawingContext drawingContext)
		{
			drawingContext.PushOpacity(_opacity);
			drawingContext.DrawRectangle(_brush, null, new Rect(Position, _ghostSize));
			drawingContext.Pop();
		}
	}
}