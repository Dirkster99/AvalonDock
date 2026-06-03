using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the anchorable Pane Tab Panel.
	/// </summary>
	public class AnchorablePaneTabPanel : Panel
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AnchorablePaneTabPanel"/> class.
		/// </summary>
		public AnchorablePaneTabPanel()
		{
			this.FlowDirection = System.Windows.FlowDirection.LeftToRight;
		}

		/// <inheritdoc/>
		protected override Size MeasureOverride(Size availableSize)
		{
			double totWidth = 0;
			double maxHeight = 0;
			var visibleChildren = Children.Cast<UIElement>().Where(ch => ch.Visibility != System.Windows.Visibility.Collapsed);
			foreach (FrameworkElement child in visibleChildren)
			{
				child.Measure(new Size(double.PositiveInfinity, availableSize.Height));
				totWidth += child.DesiredSize.Width;
				maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
			}

			if (totWidth > availableSize.Width)
			{
				double childFinalDesideredWidth = availableSize.Width / visibleChildren.Count();
				foreach (FrameworkElement child in visibleChildren)
				{
					child.Measure(new Size(childFinalDesideredWidth, availableSize.Height));
				}
			}

			return new Size(Math.Min(availableSize.Width, totWidth), maxHeight);
		}

		/// <inheritdoc/>
		protected override Size ArrangeOverride(Size finalSize)
		{
			var visibleChildren = Children.Cast<UIElement>().Where(ch => ch.Visibility != System.Windows.Visibility.Collapsed);

			double finalWidth = finalSize.Width;
			double desideredWidth = visibleChildren.Sum(ch => ch.DesiredSize.Width);
			double offsetX = 0.0;

			if (finalWidth > desideredWidth)
			{
				foreach (FrameworkElement child in visibleChildren)
				{
					double childFinalWidth = child.DesiredSize.Width;
					child.Arrange(new Rect(offsetX, 0, childFinalWidth, finalSize.Height));

					offsetX += childFinalWidth;
				}
			}
			else
			{
				double childFinalWidth = finalWidth / visibleChildren.Count();
				foreach (FrameworkElement child in visibleChildren)
				{
					child.Arrange(new Rect(offsetX, 0, childFinalWidth, finalSize.Height));

					offsetX += childFinalWidth;
				}
			}

			return finalSize;
		}

		/// <inheritdoc/>
		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
		{
			if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
				LayoutAnchorableTabItem.IsDraggingItem())
			{
				var contentModel = LayoutAnchorableTabItem.GetDraggingItem().Model as LayoutAnchorable;
				var manager = contentModel.Root.Manager;
				LayoutAnchorableTabItem.ResetDraggingItem();

				manager.StartDraggingFloatingWindowForContent(contentModel);
			}

			base.OnMouseLeave(e);
		}
	}
}