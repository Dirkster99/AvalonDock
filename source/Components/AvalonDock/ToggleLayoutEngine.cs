using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Core;
using AvalonDock.Layout;

namespace AvalonDock
{
	/// <summary>
	/// <see cref="ILayoutEngine"/> implementation for the <see cref="ToggleDockingManager"/>.
	/// Extends the generic interface with zone-aware operations that use <see cref="DockZone"/>
	/// for finer-grained control over pane placement (e.g. LeftTop vs LeftBottom).
	/// <para>
	/// All methods operate on the layout model only (no WPF visual tree), making them
	/// independently testable.
	/// </para>
	/// </summary>
	public class ToggleLayoutEngine : ILayoutEngine
	{
		/// <summary>
		/// Returns <c>true</c> if the zone is a bottom zone (BottomLeft or BottomRight).
		/// </summary>
		/// <param name="zone">The dock zone to test.</param>
		/// <returns><c>true</c> if the zone is a bottom zone; otherwise <c>false</c>.</returns>
		public static bool IsBottomZone(DockZone zone)
		{
			return zone == DockZone.BottomLeft || zone == DockZone.BottomRight;
		}

		/// <summary>
		/// Returns <c>true</c> if the zone is a left zone (LeftTop or LeftBottom).
		/// </summary>
		/// <param name="zone">The dock zone to test.</param>
		/// <returns><c>true</c> if the zone is a left zone; otherwise <c>false</c>.</returns>
		public static bool IsLeftZone(DockZone zone)
		{
			return zone == DockZone.LeftTop || zone == DockZone.LeftBottom;
		}

		/// <summary>
		/// Returns <c>true</c> if the zone is a right zone (RightTop or RightBottom).
		/// </summary>
		/// <param name="zone">The dock zone to test.</param>
		/// <returns><c>true</c> if the zone is a right zone; otherwise <c>false</c>.</returns>
		public static bool IsRightZone(DockZone zone)
		{
			return zone == DockZone.RightTop || zone == DockZone.RightBottom;
		}

		/// <summary>
		/// Maps a <see cref="DockZone"/> to the corresponding <see cref="AnchorSide"/>.
		/// </summary>
		/// <param name="zone">The dock zone.</param>
		/// <returns>The anchor side for the zone.</returns>
		public static AnchorSide ZoneToAnchorSide(DockZone zone)
		{
			if (IsLeftZone(zone)) return AnchorSide.Left;
			if (IsRightZone(zone)) return AnchorSide.Right;
			return AnchorSide.Bottom;
		}

		/// <summary>
		/// Returns the desired split orientation for panes docked in the given zone.
		/// Left/Right zones → Vertical (top-to-bottom stacking).
		/// Bottom zones → Horizontal (side-by-side stacking).
		/// </summary>
		/// <param name="zone">The dock zone.</param>
		/// <returns>The orientation for grouping panes in this zone.</returns>
		public static Orientation GetDesiredOrientationForZone(DockZone zone)
		{
			return IsBottomZone(zone) ? Orientation.Horizontal : Orientation.Vertical;
		}

		/// <inheritdoc/>
		public Orientation GetDesiredOrientation(AnchorSide side)
		{
			return side == AnchorSide.Bottom || side == AnchorSide.Top
				? Orientation.Horizontal
				: Orientation.Vertical;
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
		public void InsertPane(LayoutRoot root, LayoutAnchorablePane pane, AnchorSide side)
		{
			if (root == null) throw new ArgumentNullException(nameof(root));
			if (pane == null) throw new ArgumentNullException(nameof(pane));

			switch (side)
			{
				case AnchorSide.Left:
				{
					var hPanel = FindOrCreateContentPanel(root, Orientation.Horizontal);
					hPanel.Children.Insert(0, pane);
					break;
				}

				case AnchorSide.Right:
				{
					var hPanel = FindOrCreateContentPanel(root, Orientation.Horizontal);
					hPanel.Children.Add(pane);
					break;
				}

				case AnchorSide.Bottom:
				{
					var rootPanel = root.RootPanel;
					if (rootPanel.Orientation == Orientation.Vertical)
					{
						rootPanel.Children.Add(pane);
					}
					else
					{
						var vPanel = new LayoutPanel { Orientation = Orientation.Vertical };
						root.RootPanel = vPanel;
						vPanel.Children.Add(rootPanel);
						vPanel.Children.Add(pane);
					}

					break;
				}

				case AnchorSide.Top:
				{
					var rootPanel = root.RootPanel;
					if (rootPanel.Orientation == Orientation.Vertical)
					{
						rootPanel.Children.Insert(0, pane);
					}
					else
					{
						var vPanel = new LayoutPanel { Orientation = Orientation.Vertical };
						root.RootPanel = vPanel;
						vPanel.Children.Add(pane);
						vPanel.Children.Add(rootPanel);
					}

					break;
				}
			}
		}

		/// <summary>
		/// Inserts an anchorable pane into the correct position in the layout tree
		/// for the given dock zone. Provides finer-grained control than
		/// <see cref="InsertPane(LayoutRoot, LayoutAnchorablePane, AnchorSide)"/>
		/// by distinguishing between top/bottom zones on each side.
		/// </summary>
		/// <param name="root">The layout root.</param>
		/// <param name="pane">The pane to insert.</param>
		/// <param name="zone">The target dock zone.</param>
		public void InsertPaneForZone(LayoutRoot root, LayoutAnchorablePane pane, DockZone zone)
		{
			if (root == null) throw new ArgumentNullException(nameof(root));
			if (pane == null) throw new ArgumentNullException(nameof(pane));

			var side = ZoneToAnchorSide(zone);

			switch (side)
			{
				case AnchorSide.Right:
				{
					var hPanel = FindOrCreateContentPanel(root, Orientation.Horizontal);
					if (zone == DockZone.RightTop)
					{
						// Insert before existing right-side panes/groups
						int insertIdx = hPanel.Children.Count;
						while (insertIdx > 0 &&
							   (hPanel.Children[insertIdx - 1] is LayoutAnchorablePane ||
								hPanel.Children[insertIdx - 1] is LayoutAnchorablePaneGroup))
						{
							insertIdx--;
						}

						hPanel.Children.Insert(insertIdx, pane);
					}
					else
					{
						hPanel.Children.Add(pane);
					}

					break;
				}

				case AnchorSide.Left:
				{
					var hPanel = FindOrCreateContentPanel(root, Orientation.Horizontal);
					if (zone == DockZone.LeftBottom)
					{
						// Insert after existing left-side panes/groups
						int insertIdx = 0;
						while (insertIdx < hPanel.Children.Count &&
							   (hPanel.Children[insertIdx] is LayoutAnchorablePane ||
								hPanel.Children[insertIdx] is LayoutAnchorablePaneGroup))
						{
							insertIdx++;
						}

						hPanel.Children.Insert(insertIdx, pane);
					}
					else
					{
						hPanel.Children.Insert(0, pane);
					}

					break;
				}

				case AnchorSide.Bottom:
				{
					var rootPanel = root.RootPanel;
					if (rootPanel.Orientation == Orientation.Vertical)
					{
						if (zone == DockZone.BottomLeft)
						{
							// BottomLeft inserts before existing bottom panes/groups
							int insertIdx = rootPanel.Children.Count;
							while (insertIdx > 0 &&
								   (rootPanel.Children[insertIdx - 1] is LayoutAnchorablePane ||
									rootPanel.Children[insertIdx - 1] is LayoutAnchorablePaneGroup))
							{
								insertIdx--;
							}

							rootPanel.Children.Insert(insertIdx, pane);
						}
						else
						{
							// BottomRight appends at the end
							rootPanel.Children.Add(pane);
						}
					}
					else
					{
						var vPanel = new LayoutPanel { Orientation = Orientation.Vertical };
						root.RootPanel = vPanel;
						vPanel.Children.Add(rootPanel);
						vPanel.Children.Add(pane);
					}

					break;
				}
			}
		}

		/// <inheritdoc/>
		public void FixSplitOrientation(LayoutAnchorable anchorable, AnchorSide side)
		{
			FixSplitOrientationCore(anchorable, GetDesiredOrientation(side), side == AnchorSide.Left || side == AnchorSide.Top);
		}

		/// <summary>
		/// After an anchorable is docked, fixes the split orientation of contiguous
		/// anchorable panes. Left/Right panes are grouped vertically (top-to-bottom),
		/// Bottom panes are grouped horizontally (side-by-side).
		/// </summary>
		/// <param name="anchorable">The anchorable that was just docked.</param>
		/// <param name="zone">The dock zone it was docked into.</param>
		public void FixSplitOrientationForZone(LayoutAnchorable anchorable, DockZone zone)
		{
			// "Start variant" means this zone should be first in the group:
			// LeftTop/RightTop → top of vertical group, BottomLeft → left of horizontal group.
			bool insertAtStart = zone == DockZone.LeftTop || zone == DockZone.RightTop || zone == DockZone.BottomLeft;
			FixSplitOrientationCore(anchorable, GetDesiredOrientationForZone(zone), insertAtStart);
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

		/// <summary>
		/// Core implementation for fixing split orientation of contiguous anchorable panes.
		/// </summary>
		/// <param name="anchorable">The anchorable that was just docked.</param>
		/// <param name="desiredOrientation">The desired orientation for the group.</param>
		/// <param name="insertAtStart">Whether new panes should be inserted at the start of existing groups.</param>
		private static void FixSplitOrientationCore(LayoutAnchorable anchorable, Orientation desiredOrientation, bool insertAtStart)
		{
			if (anchorable == null) return;

			var pane = anchorable.Parent as LayoutAnchorablePane;
			if (pane == null) return;

			var parentPanel = pane.Parent as LayoutPanel;
			if (parentPanel == null) return;

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
				if (insertAtStart)
					existingGroup.Children.Insert(0, pane);
				else
					existingGroup.Children.Add(pane);

				// Update the group's cross-axis dimension to the max of all children
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

			// Set the group's cross-axis dimension to the max of all panes.
			// For vertical groups (left/right): width = max width.
			// For horizontal groups (bottom): height = max height.
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