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