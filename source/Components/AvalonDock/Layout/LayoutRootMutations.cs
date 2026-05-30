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

			ILayoutPanelElement newPane = content is LayoutAnchorable anc
				? (ILayoutPanelElement)new LayoutAnchorablePane(anc)
				: content is LayoutDocument doc
					? new LayoutDocumentPane(doc)
					: null;
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
				// Wrap existing tree in a sub-panel, add new pane alongside.
				var newPanel  = new LayoutPanel { Orientation = orient };
				var existing  = rootPanel.Children.ToList();
				rootPanel.Children.Clear();
				var sub = new LayoutPanel { Orientation = rootPanel.Orientation };
				foreach (var c in existing) sub.Children.Add(c);
				if (isBefore) { newPanel.Children.Add(newPane); newPanel.Children.Add(sub); }
				else          { newPanel.Children.Add(sub);     newPanel.Children.Add(newPane); }
				root.RootPanel = newPanel;
			}
		}

		// ── Center (tab-join) ────────────────────────────────────────────────────

		private static void InsertCenter(LayoutRoot root, LayoutContent content)
		{
			if (content is LayoutDocument doc)
			{
				// Add to the first non-floating document pane.
				var target = root.Descendents().OfType<LayoutDocumentPane>()
					.FirstOrDefault(p => p.FindParent<LayoutDocumentFloatingWindow>() == null);
				if (target == null) return;
				target.Children.Add(doc);
				target.SelectedContentIndex = target.Children.Count - 1;
			}
			else if (content is LayoutAnchorable anc)
			{
				// Add to the first non-floating anchorable pane (tab-join).
				var target = root.Descendents().OfType<LayoutAnchorablePane>()
					.FirstOrDefault(p => p.FindParent<LayoutAnchorableFloatingWindow>() == null);
				if (target != null)
				{
					target.Children.Add(anc);
					target.SelectedContentIndex = target.Children.Count - 1;
				}
				else
				{
					// No anchorable pane exists — create one next to the document area.
					var docPane = root.Descendents().OfType<LayoutDocumentPane>()
						.FirstOrDefault(p => p.FindParent<LayoutDocumentFloatingWindow>() == null);
					var newPane = new LayoutAnchorablePane(anc);
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
			bool isHorizontal = zone == CompassDropZone.Left  || zone == CompassDropZone.Right;
			bool isBefore     = zone == CompassDropZone.Left  || zone == CompassDropZone.Top;
			var  orient       = isHorizontal ? Orientation.Horizontal : Orientation.Vertical;

			ILayoutPanelElement newPane = content is LayoutDocument d
				? (ILayoutPanelElement)new LayoutDocumentPane(d)
				: content is LayoutAnchorable a
					? new LayoutAnchorablePane(a)
					: null;
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
				if (parentPanel.Orientation == orient)
				{
					parentPanel.InsertChildAt(isBefore ? idx : idx + 1, newPane);
				}
				else
				{
					parentPanel.RemoveChild(anchorPane);
					var sub = new LayoutPanel { Orientation = orient };
					if (isBefore) { sub.Children.Add(newPane); sub.Children.Add(anchorPane); }
					else          { sub.Children.Add(anchorPane); sub.Children.Add(newPane); }
					parentPanel.InsertChildAt(idx, sub);
				}
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
				var newPanel = new LayoutPanel { Orientation = orient };
				var existing = rootPanel.Children.ToList();
				rootPanel.Children.Clear();
				var sub = new LayoutPanel { Orientation = rootPanel.Orientation };
				foreach (var c in existing) sub.Children.Add(c);
				if (isBefore) { newPanel.Children.Add(newPane); newPanel.Children.Add(sub); }
				else          { newPanel.Children.Add(sub);     newPanel.Children.Add(newPane); }
				root.RootPanel = newPanel;
			}
		}
	}
}
