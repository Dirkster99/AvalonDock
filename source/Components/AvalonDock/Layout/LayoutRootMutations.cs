// Structural layout mutations — shared between the WPF build and the Uno port.
// All methods operate purely on the LayoutRoot / LayoutPanel / LayoutDocumentPane /
// LayoutAnchorablePane model and have zero UI dependency.

using System.Linq;
using System.Windows.Controls;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Drop zones for the compass overlay.
	/// Matches AvalonDock's DropTargetType semantics, grouped by area:
	///   • Center / Left / Right / Top / Bottom  — inner compass, relative to the hovered pane
	///   • OuterLeft / OuterRight / OuterTop / OuterBottom — outer edge of the DockingManager
	/// </summary>
	public enum CompassDropZone
	{
		/// <summary>No zone (cursor outside all indicators).</summary>
		None,

		// ── Inner compass (5 zones, centered over the hovered pane) ─────────────
		/// <summary>Tab-join: add as a tab in the target pane.</summary>
		Center,
		/// <summary>Split left within the document / anchorable area.</summary>
		Left,
		/// <summary>Split right within the document / anchorable area.</summary>
		Right,
		/// <summary>Split above within the document / anchorable area.</summary>
		Top,
		/// <summary>Split below within the document / anchorable area.</summary>
		Bottom,

		// ── Outer manager edge (4 zones, shown at the absolute edges of the DockingManager) ──
		/// <summary>Dock a new anchorable pane at the left edge of the manager.</summary>
		OuterLeft,
		/// <summary>Dock a new anchorable pane at the right edge of the manager.</summary>
		OuterRight,
		/// <summary>Dock a new anchorable pane at the top edge of the manager.</summary>
		OuterTop,
		/// <summary>Dock a new anchorable pane at the bottom edge of the manager.</summary>
		OuterBottom,
	}

	/// <summary>
	/// Pure-model layout mutations shared between the WPF and Uno builds.
	/// Zero UI dependency — all callers pass a <see cref="CompassDropZone"/> resolved by
	/// the platform-specific overlay.
	/// </summary>
	public static class LayoutRootMutations
	{
		/// <summary>
		/// Apply a drop against explicit active targets selected by the UI layer.
		/// Handles Center (tab-join) and inner split (Left/Right/Top/Bottom) semantics.
		/// </summary>
		public static bool TryInsertToActiveTarget(
			LayoutContent content,
			LayoutDocumentPane activeDocumentPane,
			int? activeDocumentTabInsertIndex,
			LayoutAnchorablePane activeAnchorablePane,
			int? activeAnchorableTabInsertIndex,
			CompassDropZone zone)
		{
			if (content == null || zone == CompassDropZone.None)
				return false;

			if (zone == CompassDropZone.Center)
			{
				if (activeAnchorablePane != null && content is LayoutAnchorable)
				{
					return TryInsertIntoTargetPane(content, activeAnchorablePane, activeAnchorableTabInsertIndex);
				}

				if (activeDocumentPane != null && content is LayoutDocument)
				{
					return TryInsertIntoTargetPane(content, activeDocumentPane, activeDocumentTabInsertIndex);
				}

				return false;
			}

			var targetElement = (ILayoutPanelElement)activeDocumentPane ?? activeAnchorablePane;
			if (targetElement == null)
				return false;

			return TryInsertBesideTarget(content, targetElement, zone);
		}

		/// <summary>
		/// Insert <paramref name="content"/> into a concrete target pane as a tab-join
		/// (Center drop semantics). Optional <paramref name="tabInsertIndex"/> controls
		/// insertion position; when null, content is appended.
		/// </summary>
		public static bool TryInsertIntoTargetPane(LayoutContent content, ILayoutPanelElement targetPane, int? tabInsertIndex = null)
		{
			if (content == null || targetPane == null)
				return false;

			if (content is LayoutDocument doc && targetPane is LayoutDocumentPane documentPane)
			{
				if (tabInsertIndex.HasValue)
				{
					var index = tabInsertIndex.Value;
					if (index < 0) index = 0;
					if (index > documentPane.ChildrenCount) index = documentPane.ChildrenCount;
					documentPane.Children.Insert(index, doc);
				}
				else
				{
					documentPane.Children.Add(doc);
				}

				documentPane.SelectedContentIndex = documentPane.ChildrenCount - 1;
				return true;
			}

			if (content is LayoutAnchorable anc && targetPane is LayoutAnchorablePane anchorablePane)
			{
				if (tabInsertIndex.HasValue)
				{
					var index = tabInsertIndex.Value;
					if (index < 0) index = 0;
					if (index > anchorablePane.ChildrenCount) index = anchorablePane.ChildrenCount;
					anchorablePane.Children.Insert(index, anc);
				}
				else
				{
					anchorablePane.Children.Add(anc);
				}

				anchorablePane.SelectedContentIndex = anchorablePane.ChildrenCount - 1;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Insert <paramref name="content"/> beside a concrete <paramref name="targetElement"/>
		/// according to an inner split <paramref name="zone"/> (Left/Right/Top/Bottom).
		/// Returns <c>false</c> when inputs are invalid or the target has no panel parent.
		/// </summary>
		public static bool TryInsertBesideTarget(LayoutContent content, ILayoutPanelElement targetElement, CompassDropZone zone)
		{
			if (content == null || targetElement == null)
				return false;

			if (zone != CompassDropZone.Left && zone != CompassDropZone.Right &&
				zone != CompassDropZone.Top && zone != CompassDropZone.Bottom)
			{
				return false;
			}

			var newPane = CreatePaneForContent(content);
			if (newPane == null)
				return false;

			var parentPanel = targetElement.Parent as LayoutPanel;
			if (parentPanel == null)
				return false;

			var orientation = (zone == CompassDropZone.Left || zone == CompassDropZone.Right)
				? Orientation.Horizontal
				: Orientation.Vertical;
			var insertBefore = zone == CompassDropZone.Left || zone == CompassDropZone.Top;
			var targetIndex = parentPanel.Children.ToList().IndexOf(targetElement);

			if (targetIndex < 0)
				return false;

			InsertRelativeToTarget(parentPanel, targetElement, targetIndex, newPane, orientation, insertBefore);

			return true;
		}

		/// <summary>
		/// Insert <paramref name="content"/> into the layout according to <paramref name="zone"/>.
		///
		/// Inner zones (Center, Left, Right, Top, Bottom):
		///   Anchor to the first non-floating <see cref="LayoutDocumentPane"/> for documents,
		///   or the first non-floating <see cref="LayoutAnchorablePane"/> for anchorables.
		///   Keeps tool-window panes on the outer edges (VS behaviour).
		///
		/// Outer zones (OuterLeft/Right/Top/Bottom):
		///   Insert at the absolute outer edge of the <see cref="LayoutRoot.RootPanel"/>,
		///   always as a <see cref="LayoutAnchorablePane"/>.
		/// </summary>
		public static void InsertPane(LayoutRoot root, LayoutContent content, CompassDropZone zone)
		{
			if (root?.RootPanel == null || content == null) return;
			if (zone == CompassDropZone.None) return;

			// ── Outer manager-edge zones ─────────────────────────────────────────
			if (zone == CompassDropZone.OuterLeft  || zone == CompassDropZone.OuterRight ||
			    zone == CompassDropZone.OuterTop   || zone == CompassDropZone.OuterBottom)
			{
				InsertAtOuterEdge(root, content, zone);
				return;
			}

			// ── Center: tab-join ─────────────────────────────────────────────────
			if (zone == CompassDropZone.Center)
			{
				InsertCenter(root, content);
				return;
			}

			// ── Inner directional split ──────────────────────────────────────────
			InsertInnerSplit(root, content, zone);
		}

		// ── Outer-edge insertion ─────────────────────────────────────────────────

		private static void InsertAtOuterEdge(LayoutRoot root, LayoutContent content, CompassDropZone zone)
		{
			bool isHorizontal = zone == CompassDropZone.OuterLeft || zone == CompassDropZone.OuterRight;
			bool isBefore     = zone == CompassDropZone.OuterLeft || zone == CompassDropZone.OuterTop;
			var  orient       = isHorizontal ? Orientation.Horizontal : Orientation.Vertical;

			var newPane = CreatePaneForContent(content);
			if (newPane == null) return;

			var rootPanel = root.RootPanel;
			if (rootPanel.Orientation == orient)
			{
				// Same orientation — insert at the edge.
				if (isBefore) rootPanel.Children.Insert(0, newPane);
				else          rootPanel.Children.Add(newPane);
			}
			else
			{
				ReplaceRootWithSplit(root, newPane, orient, isBefore);
			}
		}

		// ── Center (tab-join) ────────────────────────────────────────────────────

		private static void InsertCenter(LayoutRoot root, LayoutContent content)
		{
			if (content is LayoutDocument)
			{
				// Add to the first non-floating document pane.
				var target = root.Descendents().OfType<LayoutDocumentPane>()
					.FirstOrDefault(p => p.FindParent<LayoutDocumentFloatingWindow>() == null);
				if (target == null) return;
				_ = TryInsertIntoTargetPane(content, target);
			}
			else if (content is LayoutAnchorable)
			{
				// Add to the first non-floating anchorable pane (tab-join).
				var target = root.Descendents().OfType<LayoutAnchorablePane>()
					.FirstOrDefault(p => p.FindParent<LayoutAnchorableFloatingWindow>() == null);
				if (target != null)
				{
					_ = TryInsertIntoTargetPane(content, target);
				}
				else
				{
					// No anchorable pane exists — create one next to the document area.
					var docPane = root.Descendents().OfType<LayoutDocumentPane>()
						.FirstOrDefault(p => p.FindParent<LayoutDocumentFloatingWindow>() == null);
					var newPane = CreatePaneForContent(content);
					if (newPane == null) return;
					if (docPane?.Parent is LayoutPanel parentPanel)
					{
						var idx = parentPanel.Children.ToList().IndexOf(docPane);
						parentPanel.InsertChildAt(idx + 1, newPane);
					}
					else
					{
						root.RootPanel.Children.Add(newPane);
					}
				}
			}
		}

		// ── Inner directional split ───────────────────────────────────────────────

		private static void InsertInnerSplit(LayoutRoot root, LayoutContent content, CompassDropZone zone)
		{
			if (zone != CompassDropZone.Left && zone != CompassDropZone.Right &&
				zone != CompassDropZone.Top && zone != CompassDropZone.Bottom)
			{
				return;
			}

			bool isHorizontal = zone == CompassDropZone.Left  || zone == CompassDropZone.Right;
			bool isBefore     = zone == CompassDropZone.Left  || zone == CompassDropZone.Top;
			var  orient       = isHorizontal ? Orientation.Horizontal : Orientation.Vertical;

			var newPane = CreatePaneForContent(content);
			if (newPane == null) return;

			// Find the anchor pane: document pane for documents, anchorable pane for anchorables.
			ILayoutPanelElement anchorPane = content is LayoutDocument
				? (ILayoutPanelElement)root.Descendents().OfType<LayoutDocumentPane>()
					.FirstOrDefault(p => p.FindParent<LayoutDocumentFloatingWindow>() == null)
				: root.Descendents().OfType<LayoutAnchorablePane>()
					.FirstOrDefault(p => p.FindParent<LayoutAnchorableFloatingWindow>() == null)
				  ?? (ILayoutPanelElement)root.Descendents().OfType<LayoutDocumentPane>()
					.FirstOrDefault(p => p.FindParent<LayoutDocumentFloatingWindow>() == null);

			if (anchorPane?.Parent is LayoutPanel parentPanel)
			{
				var idx = parentPanel.Children.ToList().IndexOf(anchorPane);
				InsertRelativeToTarget(parentPanel, anchorPane, idx, newPane, orient, isBefore);
				return;
			}

			// Fallback: operate on root panel.
			var rootPanel = root.RootPanel;
			if (rootPanel.Orientation == orient)
			{
				if (isBefore) rootPanel.Children.Insert(0, newPane);
				else          rootPanel.Children.Add(newPane);
			}
			else
			{
				ReplaceRootWithSplit(root, newPane, orient, isBefore);
			}
		}

		private static void InsertRelativeToTarget(
			LayoutPanel parentPanel,
			ILayoutPanelElement targetElement,
			int targetIndex,
			ILayoutPanelElement newPane,
			Orientation orientation,
			bool insertBefore)
		{
			if (parentPanel.Orientation == orientation)
			{
				parentPanel.InsertChildAt(insertBefore ? targetIndex : targetIndex + 1, newPane);
				return;
			}

			parentPanel.RemoveChild(targetElement);
			var splitPanel = new LayoutPanel { Orientation = orientation };
			if (insertBefore)
			{
				splitPanel.Children.Add(newPane);
				splitPanel.Children.Add(targetElement);
			}
			else
			{
				splitPanel.Children.Add(targetElement);
				splitPanel.Children.Add(newPane);
			}
			parentPanel.InsertChildAt(targetIndex, splitPanel);
		}

		private static void ReplaceRootWithSplit(LayoutRoot root, ILayoutPanelElement newPane, Orientation orientation, bool insertBefore)
		{
			var rootPanel = root.RootPanel;
			var newPanel = new LayoutPanel { Orientation = orientation };
			var existing = rootPanel.Children.ToList();
			rootPanel.Children.Clear();
			var sub = new LayoutPanel { Orientation = rootPanel.Orientation };
			foreach (var c in existing)
				sub.Children.Add(c);

			if (insertBefore)
			{
				newPanel.Children.Add(newPane);
				newPanel.Children.Add(sub);
			}
			else
			{
				newPanel.Children.Add(sub);
				newPanel.Children.Add(newPane);
			}

			root.RootPanel = newPanel;
		}

		private static ILayoutPanelElement CreatePaneForContent(LayoutContent content)
		{
			return content is LayoutDocument doc
				? (ILayoutPanelElement)new LayoutDocumentPane(doc)
				: content is LayoutAnchorable anc
					? new LayoutAnchorablePane(anc)
					: null;
		}
	}
}
