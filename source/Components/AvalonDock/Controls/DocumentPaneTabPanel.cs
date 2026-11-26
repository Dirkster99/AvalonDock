using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the document Pane Tab Panel.
	/// </summary>
	public class DocumentPaneTabPanel : Panel
	{
		/// <summary><see cref="WrapTabs"/> dependency property.</summary>
		public static readonly DependencyProperty WrapTabsProperty = DependencyProperty.Register(nameof(WrapTabs), typeof(bool), typeof(DocumentPaneTabPanel),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

		/// <summary>
		/// Gets or sets a value indicating whether gets or sets whether tabs wrap onto additional rows when they do not fit within the available width.
		/// </summary>
		public bool WrapTabs
		{
			get => (bool)GetValue(WrapTabsProperty);
			set => SetValue(WrapTabsProperty, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentPaneTabPanel"/> class.
		/// </summary>
		public DocumentPaneTabPanel()
		{
			this.FlowDirection = System.Windows.FlowDirection.LeftToRight;
		}

		/// <inheritdoc/>
		protected override Size MeasureOverride(Size availableSize)
		{
			if (WrapTabs)
				return MeasureWrappedTabs(availableSize);

			Size desideredSize = new Size();
			foreach (FrameworkElement child in Children)
			{
				child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
				desideredSize.Width += child.DesiredSize.Width;

				desideredSize.Height = Math.Max(desideredSize.Height, child.DesiredSize.Height);
			}

			return new Size(Math.Min(desideredSize.Width, availableSize.Width), desideredSize.Height);
		}

		/// <inheritdoc/>
		protected override Size ArrangeOverride(Size finalSize)
		{
			if (WrapTabs)
				return ArrangeWrappedTabs(finalSize);

			var visibleChildren = Children.Cast<UIElement>().Where(ch => ch.Visibility != System.Windows.Visibility.Collapsed);
			var offset = 0.0;
			var skipAllOthers = false;
			foreach (TabItem doc in visibleChildren)
			{
				if (skipAllOthers || offset + doc.DesiredSize.Width > finalSize.Width)
				{
					bool isLayoutContentSelected = false;
					var layoutContent = doc.Content as LayoutContent;

					if (layoutContent != null)
						isLayoutContentSelected = layoutContent.IsSelected;

					if (isLayoutContentSelected && !doc.IsVisible)
					{
						var parentContainer = layoutContent.Parent as ILayoutContainer;
						var parentSelector = layoutContent.Parent as ILayoutContentSelector;
						var parentPane = layoutContent.Parent as ILayoutPane;
						int contentIndex = parentSelector.IndexOf(layoutContent);
						if (contentIndex > 0 &&
							parentContainer.ChildrenCount > 1)
						{
							parentPane.MoveChild(contentIndex, 0);
							parentSelector.SelectedContentIndex = 0;
							return ArrangeOverride(finalSize);
						}
					}

					doc.Visibility = System.Windows.Visibility.Hidden;
					skipAllOthers = true;
				}
				else
				{
					doc.Visibility = System.Windows.Visibility.Visible;
					doc.Arrange(new Rect(offset, 0.0, doc.DesiredSize.Width, finalSize.Height));
					offset += doc.ActualWidth + doc.Margin.Left + doc.Margin.Right;
				}
			}

			return finalSize;
		}

		private Size MeasureWrappedTabs(Size availableSize)
		{
			double rowWidth = 0;
			double rowHeight = 0;
			double desiredWidth = 0;
			double desiredHeight = 0;
			var availableWidth = availableSize.Width;

			foreach (FrameworkElement child in Children.Cast<FrameworkElement>().Where(child => child.Visibility != Visibility.Collapsed))
			{
				child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
				var childSize = child.DesiredSize;

				if (!double.IsPositiveInfinity(availableWidth) && rowWidth > 0 && rowWidth + childSize.Width > availableWidth)
				{
					desiredWidth = Math.Max(desiredWidth, rowWidth);
					desiredHeight += rowHeight;
					rowWidth = 0;
					rowHeight = 0;
				}

				rowWidth += childSize.Width;
				rowHeight = Math.Max(rowHeight, childSize.Height);
			}

			desiredWidth = Math.Max(desiredWidth, rowWidth);
			desiredHeight += rowHeight;
			return new Size(Math.Min(desiredWidth, availableWidth), desiredHeight);
		}

		private Size ArrangeWrappedTabs(Size finalSize)
		{
			double offsetX = 0;
			double offsetY = 0;
			double rowHeight = 0;

			foreach (FrameworkElement child in Children.Cast<FrameworkElement>().Where(child => child.Visibility != Visibility.Collapsed))
			{
				var childSize = child.DesiredSize;
				if (offsetX > 0 && offsetX + childSize.Width > finalSize.Width)
				{
					offsetX = 0;
					offsetY += rowHeight;
					rowHeight = 0;
				}

				child.Visibility = Visibility.Visible;
				child.Arrange(new Rect(offsetX, offsetY, childSize.Width, childSize.Height));
				offsetX += childSize.Width;
				rowHeight = Math.Max(rowHeight, childSize.Height);
			}

			return finalSize;
		}

		/// <inheritdoc/>
		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
		{
			// if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
			//    LayoutDocumentTabItem.IsDraggingItem())
			// {
			//    var contentModel = LayoutDocumentTabItem.GetDraggingItem().Model;
			//    var manager = contentModel.Root.Manager;
			//    LayoutDocumentTabItem.ResetDraggingItem();
			//    System.Diagnostics.Trace.WriteLine("OnMouseLeave()");

			// manager.StartDraggingFloatingWindowForContent(contentModel);
			// }
			base.OnMouseLeave(e);
		}
	}
}