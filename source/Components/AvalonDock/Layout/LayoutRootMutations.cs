// Structural layout mutations — shared between the WPF build and the Uno port.
// All methods operate purely on the LayoutRoot / LayoutPanel / LayoutDocumentPane
// model and have zero UI dependency.
//
// Linked from UnoDock.csproj (HAS_UNO builds) via:
//   <Compile Include="..\..\externals\...\Layout\LayoutRootMutations.cs" Link="Layout\LayoutRootMutations.cs" />

using System.Linq;
using System.Windows.Controls;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Pure-model mutations shared between the WPF and Uno builds.
	/// The Uno DockingManager calls these instead of duplicating the logic.
	/// </summary>
	public static class LayoutRootMutations
	{
		/// <summary>
		/// Insert <paramref name="content"/> into the layout adjacent to the first
		/// non-floating <see cref="LayoutDocumentPane"/> (compass drop zones).
		///
		/// For Left/Right/Top/Bottom: the new pane is inserted next to the existing
		/// document pane — keeping tool-window panes on the outer edges (VS behaviour).
		/// Falls back to a root-level split when no panel parent is found.
		/// For Center: adds directly into the target document pane.
		/// </summary>
		public static void InsertPane(LayoutRoot root, LayoutContent content, CompassDropZone zone)
		{
			if (root?.RootPanel == null || content == null) return;

			if (zone == CompassDropZone.Center)
			{
				var target = root.Descendents().OfType<LayoutDocumentPane>()
					.FirstOrDefault(p => p.FindParent<LayoutDocumentFloatingWindow>() == null);
				if (target == null) return;
				if (content is LayoutDocument doc) target.Children.Add(doc);
				else if (content is LayoutAnchorable anc) { var ap = new LayoutAnchorablePane(anc); root.RootPanel.Children.Add(ap); }
				return;
			}

			bool isHorizontal = zone == CompassDropZone.Left  || zone == CompassDropZone.Right;
			bool isBefore     = zone == CompassDropZone.Left  || zone == CompassDropZone.Top;
			var  targetOrient = isHorizontal ? Orientation.Horizontal : Orientation.Vertical;

			ILayoutPanelElement newPane = content is LayoutDocument d
				? (ILayoutPanelElement)new LayoutDocumentPane(d)
				: content is LayoutAnchorable a
					? new LayoutAnchorablePane(a)
					: null;
			if (newPane == null) return;

			// Find the first non-floating document pane as the insertion anchor.
			var targetDocPane = root.Descendents()
				.OfType<LayoutDocumentPane>()
				.FirstOrDefault(p => p.FindParent<LayoutDocumentFloatingWindow>() == null);

			if (targetDocPane?.Parent is LayoutPanel parentPanel)
			{
				var idx = parentPanel.Children.ToList().IndexOf(targetDocPane);
				if (parentPanel.Orientation == targetOrient)
				{
					parentPanel.InsertChildAt(isBefore ? idx : idx + 1, newPane);
				}
				else
				{
					parentPanel.RemoveChild(targetDocPane);
					var sub = new LayoutPanel { Orientation = targetOrient };
					if (isBefore) { sub.Children.Add(newPane); sub.Children.Add(targetDocPane); }
					else          { sub.Children.Add(targetDocPane); sub.Children.Add(newPane); }
					parentPanel.InsertChildAt(idx, sub);
				}
				return;
			}

			// Fallback: operate on the root panel.
			var rootPanel = root.RootPanel;
			if (rootPanel.Orientation == targetOrient)
			{
				if (isBefore) rootPanel.Children.Insert(0, newPane);
				else          rootPanel.Children.Add(newPane);
			}
			else
			{
				var newPanel = new LayoutPanel { Orientation = targetOrient };
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

	/// <summary>Drop zones for the compass overlay — shared between WPF and Uno.</summary>
	public enum CompassDropZone { Center, Left, Right, Top, Bottom }
}
