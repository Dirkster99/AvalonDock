using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using AvalonDock.Layout;


namespace AvalonDock.Controls
{
	internal static class LayoutFloatingWindowControlHelper
	{
		private const string Excp_NotSupportedFloatingWindowType = "Not Supported Floating Window Type: {0}";

		public static void ActiveTheContentOfSinglePane<T>(T fwc, bool isActive) where T : LayoutFloatingWindowControl
		{
			ILayoutContentSelector selector = null;
			if (fwc is LayoutAnchorableFloatingWindowControl)
			{
				selector = fwc.Model
					.Descendents()
						.OfType<LayoutAnchorablePane>()
							.FirstOrDefault(p => p.ChildrenCount > 0 && p.SelectedContent != null);
			}
			else if (fwc is LayoutDocumentFloatingWindowControl)
			{
				selector = fwc.Model
					.Descendents()
						.OfType<LayoutDocumentPane>()
							.FirstOrDefault(p => p.ChildrenCount > 0 && p.SelectedContent != null);
			}
			else
			{
				throw new NotSupportedException(string.Format(Excp_NotSupportedFloatingWindowType, fwc.GetType()));
			}

			if (selector != null)
			{
				selector.SelectedContent.IsActive = isActive;
			}
			else
			{
				// When the floating tool window is mixed with the floating document window
				// and the document pane in the floating document window is dragged out.

				// Only the Tool panes is left in the floating document window.
				// The Children Count is greater than 0 and the Selected Content is null.

				// Then we only need to activate the last active content.
				ActiveTheLastActivedContent(fwc, isActive);
			}
		}

		public static void ActiveTheContentOfMultiPane<T>(T fwc, bool isActive) where T : LayoutFloatingWindowControl
		{
			if (isActive)
			{
				if (fwc is LayoutAnchorableFloatingWindowControl)
				{
					var paneControl = GetLayoutControlByMousePosition<LayoutAnchorablePaneControl>(fwc);
					if (paneControl != null && paneControl.Model is LayoutAnchorablePane pane)
					{
						if (pane.SelectedContent != null)
							pane.SelectedContent.IsActive = true;
						else
							ActiveTheLastActivedContentOfPane(pane);

						return;
					}
				}
				else if (fwc is LayoutDocumentFloatingWindowControl)
				{
					var paneControl = GetLayoutControlByMousePosition<LayoutDocumentPaneControl>(fwc);
					if (paneControl != null && paneControl.Model is LayoutDocumentPane pane)
					{
						if (pane.SelectedContent != null)
							pane.SelectedContent.IsActive = true;
						else
							ActiveTheLastActivedContentOfPane(pane);

						return;
					}
				}
				else
				{
					throw new NotSupportedException(string.Format(Excp_NotSupportedFloatingWindowType, fwc.GetType()));
				}
			}

			ActiveTheLastActivedContent(fwc, isActive);
		}

		public static void ActiveTheLastActivedContent(LayoutFloatingWindowControl fwc, bool isActive)
		{
			var items = fwc.Model.Descendents().OfType<LayoutContent>().ToList();
			var index = IndexOfLastActivedContent(items);
			if (index != -1)
			{
				items[index].IsActive = isActive;
			}
		}

		public static void ActiveTheLastActivedContentOfPane(LayoutAnchorablePane anchorablePane)
		{
			var index = IndexOfLastActivedContent(anchorablePane.Children);
			if (index != -1)
			{
				anchorablePane.SelectedContentIndex = index;
				if (!anchorablePane.SelectedContent.IsActive)
				{
					anchorablePane.SelectedContent.IsActive = true;
				}
			}
		}

		public static void ActiveTheLastActivedContentOfPane(LayoutDocumentPane documentPane)
		{
			var index = IndexOfLastActivedContent(documentPane.Children);
			if (index != -1)
			{
				documentPane.SelectedContentIndex = index;
				if (!documentPane.SelectedContent.IsActive)
				{
					documentPane.SelectedContent.IsActive = true;
				}
			}
		}

		private static T GetLayoutControlByMousePosition<T>(LayoutFloatingWindowControl fwc) where T : FrameworkElement, ILayoutControl
		{
			var mousePosition = fwc.PointToScreenDPI(Mouse.GetPosition(fwc));
			var rootVisual = ((LayoutFloatingWindowControl.FloatingWindowContentHost)fwc.Content).RootVisual;

			foreach (var areaHost in rootVisual.FindVisualChildren<T>())
			{
				var rect = areaHost.GetScreenArea();
				if (rect.Contains(mousePosition))
				{
					return areaHost;
				}
			}

			return null;
		}

		private static int IndexOfLastActivedContent<T>(IList<T> list) where T : LayoutContent
		{
			if (list.Count > 0)
			{
				var index = 0;
				if (list.Count > 1)
				{
					var tmpTimeStamp = list[0].LastActivationTimeStamp;
					for (var i = 1; i < list.Count; i++)
					{
						var item = list[i];
						if (item.LastActivationTimeStamp > tmpTimeStamp)
						{
							tmpTimeStamp = item.LastActivationTimeStamp;
							index = i;
						}
					}
				}

				return index;
			}

			return -1;
		}
	}
}
