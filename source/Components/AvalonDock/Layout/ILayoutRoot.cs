using System.Collections.ObjectModel;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Interface for layout root.
	/// </summary>
	public interface ILayoutRoot
	{
		/// <summary>
		/// Gets the manager.
		/// </summary>
		DockingManager Manager { get; }

		/// <summary>
		/// Gets the root panel.
		/// </summary>
		LayoutPanel RootPanel { get; }

		/// <summary>
		/// Gets the top side.
		/// </summary>
		LayoutAnchorSide TopSide { get; }

		/// <summary>
		/// Gets the left side.
		/// </summary>
		LayoutAnchorSide LeftSide { get; }

		/// <summary>
		/// Gets the right side.
		/// </summary>
		LayoutAnchorSide RightSide { get; }

		/// <summary>
		/// Gets the bottom side.
		/// </summary>
		LayoutAnchorSide BottomSide { get; }

		/// <summary>
		/// Gets or sets the active content.
		/// </summary>
		LayoutContent ActiveContent { get; set; }

		/// <summary>
		/// Gets the floating windows.
		/// </summary>
		ObservableCollection<LayoutFloatingWindow> FloatingWindows { get; }

		/// <summary>
		/// Gets the hidden.
		/// </summary>
		ObservableCollection<LayoutAnchorable> Hidden { get; }

		/// <summary>
		/// Collects the garbage.
		/// </summary>
		void CollectGarbage();
	}
}