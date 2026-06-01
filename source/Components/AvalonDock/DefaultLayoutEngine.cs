using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;

namespace AvalonDock
{
	/// <summary>
	/// Default implementation of <see cref="ILayoutEngine"/> that matches the conventional
	/// <see cref="DockingManager"/> behavior. Inserts panes with simple orientation-based logic
	/// (if the root panel orientation matches the target side, append/insert directly;
	/// otherwise wrap the root in a new panel).
	/// </summary>
	public class DefaultLayoutEngine : ILayoutEngine
	{
		/// <inheritdoc/>
		public void InsertPane(LayoutRoot root, LayoutAnchorablePane pane, AnchorSide side)
		{
			if (root == null) throw new ArgumentNullException(nameof(root));
			if (pane == null) throw new ArgumentNullException(nameof(pane));

			var rootPanel = root.RootPanel;

			switch (side)
			{
				case AnchorSide.Left:
					if (rootPanel.Orientation == Orientation.Horizontal)
					{
						rootPanel.Children.Insert(0, pane);
					}
					else
					{
						var panel = new LayoutPanel { Orientation = Orientation.Horizontal };
						root.RootPanel = panel;
						panel.Children.Add(pane);
						panel.Children.Add(rootPanel);
					}

					break;

				case AnchorSide.Right:
					if (rootPanel.Orientation == Orientation.Horizontal)
					{
						rootPanel.Children.Add(pane);
					}
					else
					{
						var panel = new LayoutPanel { Orientation = Orientation.Horizontal };
						root.RootPanel = panel;
						panel.Children.Add(rootPanel);
						panel.Children.Add(pane);
					}

					break;

				case AnchorSide.Top:
					if (rootPanel.Orientation == Orientation.Vertical)
					{
						rootPanel.Children.Insert(0, pane);
					}
					else
					{
						var panel = new LayoutPanel { Orientation = Orientation.Vertical };
						root.RootPanel = panel;
						panel.Children.Add(pane);
						panel.Children.Add(rootPanel);
					}

					break;

				case AnchorSide.Bottom:
					if (rootPanel.Orientation == Orientation.Vertical)
					{
						rootPanel.Children.Add(pane);
					}
					else
					{
						var panel = new LayoutPanel { Orientation = Orientation.Vertical };
						root.RootPanel = panel;
						panel.Children.Add(rootPanel);
						panel.Children.Add(pane);
					}

					break;
			}
		}

		/// <inheritdoc/>
		public LayoutPanel FindOrCreateContentPanel(LayoutRoot root, Orientation orientation)
		{
			if (root == null) throw new ArgumentNullException(nameof(root));

			var rootPanel = root.RootPanel;
			if (rootPanel.Orientation == orientation)
				return rootPanel;

			// Look for an existing child panel with the desired orientation
			foreach (var child in rootPanel.Children)
			{
				if (child is LayoutPanel panel && panel.Orientation == orientation)
					return panel;
			}

			// Wrap the first non-anchorable child in a new panel
			var newPanel = new LayoutPanel { Orientation = orientation };

			ILayoutPanelElement contentChild = null;
			for (int i = 0; i < rootPanel.Children.Count; i++)
			{
				var child = rootPanel.Children[i];
				if (!(child is LayoutAnchorablePane))
				{
					contentChild = child;
					break;
				}
			}

			if (contentChild != null)
			{
				int idx = rootPanel.Children.IndexOf(contentChild);
				rootPanel.Children.Remove(contentChild);
				newPanel.Children.Add(contentChild);
				rootPanel.Children.Insert(idx, newPanel);
			}
			else
			{
				root.RootPanel = newPanel;
				newPanel.Children.Add(rootPanel);
			}

			return newPanel;
		}

		/// <inheritdoc/>
		public void FixSplitOrientation(LayoutAnchorable anchorable, AnchorSide side)
		{
			if (anchorable == null) return;

			var pane = anchorable.Parent as LayoutAnchorablePane;
			if (pane == null) return;

			var parentPanel = pane.Parent as LayoutPanel;
			if (parentPanel == null) return;

			var desiredOrientation = GetDesiredOrientation(side);

			// Case 1: Adjacent LayoutAnchorablePaneGroup with matching orientation
			var paneIdx = parentPanel.Children.IndexOf(pane);
			LayoutAnchorablePaneGroup existingGroup = null;

			if (paneIdx > 0 && parentPanel.Children[paneIdx - 1] is LayoutAnchorablePaneGroup leftNeighbor
				&& leftNeighbor.Orientation == desiredOrientation)
				existingGroup = leftNeighbor;
			else if (paneIdx < parentPanel.Children.Count - 1 && parentPanel.Children[paneIdx + 1] is LayoutAnchorablePaneGroup rightNeighbor
				&& rightNeighbor.Orientation == desiredOrientation)
				existingGroup = rightNeighbor;

			if (existingGroup != null)
			{
				parentPanel.Children.Remove(pane);
				bool isStartSide = side == AnchorSide.Left || side == AnchorSide.Top;
				if (isStartSide)
					existingGroup.Children.Insert(0, pane);
				else
					existingGroup.Children.Add(pane);

				UpdateGroupDimensions(existingGroup);
				return;
			}

			// Case 2: Multiple contiguous LayoutAnchorablePane siblings — wrap in a group
			paneIdx = parentPanel.Children.IndexOf(pane);
			var contiguousPanes = new List<LayoutAnchorablePane> { pane };

			for (int i = paneIdx - 1; i >= 0; i--)
			{
				if (parentPanel.Children[i] is LayoutAnchorablePane adjPane)
					contiguousPanes.Insert(0, adjPane);
				else break;
			}

			for (int i = paneIdx + 1; i < parentPanel.Children.Count; i++)
			{
				if (parentPanel.Children[i] is LayoutAnchorablePane adjPane)
					contiguousPanes.Add(adjPane);
				else break;
			}

			if (contiguousPanes.Count < 2) return;
			if (parentPanel.Orientation == desiredOrientation) return;

			var group = new LayoutAnchorablePaneGroup { Orientation = desiredOrientation };
			int firstIdx = parentPanel.Children.IndexOf(contiguousPanes[0]);

			// Set the group's cross-axis dimension to the max of all panes
			if (desiredOrientation == Orientation.Vertical)
			{
				double maxWidth = contiguousPanes.Max(p => p.DockWidth.IsAbsolute ? p.DockWidth.Value : 0);
				double maxMinWidth = contiguousPanes.Max(p => p.DockMinWidth);
				if (maxWidth > 0)
					group.DockWidth = new GridLength(maxWidth);
				if (maxMinWidth > 0)
					group.DockMinWidth = maxMinWidth;
			}
			else
			{
				double maxHeight = contiguousPanes.Max(p => p.DockHeight.IsAbsolute ? p.DockHeight.Value : 0);
				double maxMinHeight = contiguousPanes.Max(p => p.DockMinHeight);
				if (maxHeight > 0)
					group.DockHeight = new GridLength(maxHeight);
				if (maxMinHeight > 0)
					group.DockMinHeight = maxMinHeight;
			}

			for (int i = contiguousPanes.Count - 1; i >= 0; i--)
				parentPanel.Children.Remove(contiguousPanes[i]);

			foreach (var sp in contiguousPanes)
				group.Children.Add(sp);

			parentPanel.Children.Insert(Math.Min(firstIdx, parentPanel.Children.Count), group);
		}

		/// <inheritdoc/>
		public void EnsureBottomFullWidth(LayoutRoot root)
		{
			if (root == null) return;
			var rootPanel = root.RootPanel;
			if (rootPanel == null) return;

			if (rootPanel.Orientation == Orientation.Horizontal)
			{
				var bottomPanes = new List<LayoutAnchorablePane>();
				foreach (var child in rootPanel.Children.OfType<LayoutPanel>().ToList())
				{
					if (child.Orientation == Orientation.Vertical)
					{
						foreach (var pane in child.Children.OfType<LayoutAnchorablePane>().ToList())
						{
							if (child.Children.IndexOf(pane) == child.Children.Count - 1 && child.Children.Count > 1)
								bottomPanes.Add(pane);
						}
					}
				}

				if (bottomPanes.Count > 0)
				{
					var vPanel = new LayoutPanel { Orientation = Orientation.Vertical };
					root.RootPanel = vPanel;

					foreach (var bp in bottomPanes)
						((ILayoutContainer)bp.Parent).RemoveChild(bp);

					vPanel.Children.Add(rootPanel);
					foreach (var bp in bottomPanes)
						vPanel.Children.Add(bp);
				}
			}
			else if (rootPanel.Orientation == Orientation.Vertical)
			{
				var bottomPanesToLift = new List<LayoutAnchorablePane>();
				foreach (var child in rootPanel.Children.OfType<LayoutPanel>().ToList())
				{
					if (child.Orientation == Orientation.Horizontal)
					{
						foreach (var subChild in child.Children.OfType<LayoutPanel>().ToList())
						{
							if (subChild.Orientation == Orientation.Vertical)
							{
								foreach (var pane in subChild.Children.OfType<LayoutAnchorablePane>().ToList())
								{
									if (subChild.Children.IndexOf(pane) == subChild.Children.Count - 1 && subChild.Children.Count > 1)
										bottomPanesToLift.Add(pane);
								}
							}
						}
					}
				}

				foreach (var bp in bottomPanesToLift)
				{
					((ILayoutContainer)bp.Parent).RemoveChild(bp);
					rootPanel.Children.Add(bp);
				}
			}
		}

		/// <inheritdoc/>
		public void EnsureSidesFullHeight(LayoutRoot root)
		{
			if (root == null) return;
			var rootPanel = root.RootPanel;
			if (rootPanel == null) return;

			if (rootPanel.Orientation == Orientation.Vertical)
			{
				var leftPanes = new List<LayoutAnchorablePane>();
				var rightPanes = new List<LayoutAnchorablePane>();

				foreach (var child in rootPanel.Children.OfType<LayoutPanel>().ToList())
				{
					if (child.Orientation == Orientation.Horizontal)
					{
						foreach (var pane in child.Children.OfType<LayoutAnchorablePane>().ToList())
						{
							int idx = child.Children.IndexOf(pane);
							if (idx == 0 && child.Children.Count > 1)
								leftPanes.Add(pane);
							else if (idx == child.Children.Count - 1 && child.Children.Count > 1)
								rightPanes.Add(pane);
						}
					}
				}

				if (leftPanes.Count > 0 || rightPanes.Count > 0)
				{
					var hPanel = new LayoutPanel { Orientation = Orientation.Horizontal };
					root.RootPanel = hPanel;

					foreach (var lp in leftPanes)
						((ILayoutContainer)lp.Parent).RemoveChild(lp);
					foreach (var rp in rightPanes)
						((ILayoutContainer)rp.Parent).RemoveChild(rp);

					foreach (var lp in leftPanes)
						hPanel.Children.Add(lp);
					hPanel.Children.Add(rootPanel);
					foreach (var rp in rightPanes)
						hPanel.Children.Add(rp);
				}
			}
			else if (rootPanel.Orientation == Orientation.Horizontal)
			{
				var leftPanesToLift = new List<LayoutAnchorablePane>();
				var rightPanesToLift = new List<LayoutAnchorablePane>();

				foreach (var child in rootPanel.Children.OfType<LayoutPanel>().ToList())
				{
					if (child.Orientation == Orientation.Vertical)
					{
						foreach (var subChild in child.Children.OfType<LayoutPanel>().ToList())
						{
							if (subChild.Orientation == Orientation.Horizontal)
							{
								foreach (var pane in subChild.Children.OfType<LayoutAnchorablePane>().ToList())
								{
									int idx = subChild.Children.IndexOf(pane);
									if (idx == 0 && subChild.Children.Count > 1)
										leftPanesToLift.Add(pane);
									else if (idx == subChild.Children.Count - 1 && subChild.Children.Count > 1)
										rightPanesToLift.Add(pane);
								}
							}
						}
					}
				}

				foreach (var lp in leftPanesToLift)
				{
					((ILayoutContainer)lp.Parent).RemoveChild(lp);
					rootPanel.Children.Insert(0, lp);
				}

				foreach (var rp in rightPanesToLift)
				{
					((ILayoutContainer)rp.Parent).RemoveChild(rp);
					rootPanel.Children.Add(rp);
				}
			}
		}

		/// <inheritdoc/>
		public Orientation GetDesiredOrientation(AnchorSide side)
		{
			return side == AnchorSide.Bottom || side == AnchorSide.Top
				? Orientation.Horizontal
				: Orientation.Vertical;
		}

		/// <summary>
		/// Updates the group's cross-axis dimension (DockWidth for Vertical groups,
		/// DockHeight for Horizontal groups) to the maximum of its children.
		/// </summary>
		/// <param name="group">The pane group to update.</param>
		private static void UpdateGroupDimensions(LayoutAnchorablePaneGroup group)
		{
			var panes = group.Children.OfType<LayoutAnchorablePane>().ToList();
			if (panes.Count == 0) return;

			if (group.Orientation == Orientation.Vertical)
			{
				double maxWidth = panes.Max(p => p.DockWidth.IsAbsolute ? p.DockWidth.Value : 0);
				double maxMinWidth = panes.Max(p => p.DockMinWidth);
				if (maxWidth > 0)
					group.DockWidth = new GridLength(maxWidth);
				if (maxMinWidth > 0)
					group.DockMinWidth = maxMinWidth;
			}
			else
			{
				double maxHeight = panes.Max(p => p.DockHeight.IsAbsolute ? p.DockHeight.Value : 0);
				double maxMinHeight = panes.Max(p => p.DockMinHeight);
				if (maxHeight > 0)
					group.DockHeight = new GridLength(maxHeight);
				if (maxMinHeight > 0)
					group.DockMinHeight = maxMinHeight;
			}
		}
	}
}