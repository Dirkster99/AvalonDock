using System.Windows.Controls;
using AvalonDock.Layout;

namespace AvalonDock
{
	/// <summary>
	/// Defines the strategy for manipulating the layout tree when docking, undocking,
	/// and rearranging anchorable panes within a <see cref="LayoutRoot"/>.
	/// <para>
	/// Both the conventional <see cref="DockingManager"/> and <see cref="ToggleDockingManager"/>
	/// can use an <see cref="ILayoutEngine"/> to perform layout-tree operations without
	/// depending on the WPF visual tree.
	/// </para>
	/// </summary>
	public interface ILayoutEngine
	{
		/// <summary>
		/// Inserts an anchorable pane into the layout tree at the specified side.
		/// The implementation decides where exactly the pane is placed within
		/// the panel hierarchy.
		/// </summary>
		/// <param name="root">The layout root.</param>
		/// <param name="pane">The anchorable pane to insert.</param>
		/// <param name="side">The target anchor side.</param>
		void InsertPane(LayoutRoot root, LayoutAnchorablePane pane, AnchorSide side);

		/// <summary>
		/// Finds or creates a content panel with the specified orientation.
		/// For Horizontal, this is the panel that holds left panes + document + right panes.
		/// For Vertical, this is the panel that holds top content + bottom panes.
		/// </summary>
		/// <param name="root">The layout root.</param>
		/// <param name="orientation">The desired orientation of the content panel.</param>
		/// <returns>The existing or newly created panel.</returns>
		LayoutPanel FindOrCreateContentPanel(LayoutRoot root, Orientation orientation);

		/// <summary>
		/// After docking, ensures that contiguous anchorable panes on the same side
		/// are grouped with the correct split orientation.
		/// Left/Right panes → <see cref="Orientation.Vertical"/> (top-to-bottom).
		/// Top/Bottom panes → <see cref="Orientation.Horizontal"/> (side-by-side).
		/// </summary>
		/// <param name="anchorable">The anchorable that was just docked.</param>
		/// <param name="side">The anchor side it was docked to.</param>
		void FixSplitOrientation(LayoutAnchorable anchorable, AnchorSide side);

		/// <summary>
		/// Ensures bottom panes span the full width of the layout by lifting them
		/// to the outermost vertical panel level.
		/// </summary>
		/// <param name="root">The layout root.</param>
		void EnsureBottomFullWidth(LayoutRoot root);

		/// <summary>
		/// Ensures side panes (left/right) span the full height of the layout by
		/// lifting them to the outermost horizontal panel level.
		/// </summary>
		/// <param name="root">The layout root.</param>
		void EnsureSidesFullHeight(LayoutRoot root);

		/// <summary>
		/// Gets the desired split orientation for panes on the given anchor side.
		/// </summary>
		/// <param name="side">The anchor side.</param>
		/// <returns>
		/// <see cref="Orientation.Vertical"/> for Left/Right sides,
		/// <see cref="Orientation.Horizontal"/> for Top/Bottom sides.
		/// </returns>
		Orientation GetDesiredOrientation(AnchorSide side);
	}
}