using System.Collections.Generic;

namespace AvalonDock.Core
{
	/// <summary>
	/// Represents the root of a dock layout tree.
	/// </summary>
	public interface IRootDock : IDock
	{
		/// <summary>Gets or sets the list of floating dockable windows.</summary>
		IList<IDockable>? FloatingDockables { get; set; }

		/// <summary>Gets or sets the list of pinned dockables.</summary>
		IList<IDockable>? PinnedDockables { get; set; }

		/// <summary>Gets or sets the default layout to restore when resetting.</summary>
		IDockable? DefaultLayout { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether content of this layout may be torn off into floating
		/// windows. Applied to <see cref="IDockingManager.AllowFloatingWindows"/> of the docking manager
		/// this layout is bound to, and kept in sync while it stays bound.
		/// </summary>
		/// <remarks>Defaults to <see langword="true"/>.</remarks>
		bool AllowFloatingWindows { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether anchorables of this layout may be moved into
		/// standalone top level windows. Applied to
		/// <see cref="IDockingManager.AllowDetachedWindows"/> of the docking manager this layout is
		/// bound to, and kept in sync while it stays bound.
		/// </summary>
		/// <remarks>Defaults to <see langword="true"/>.</remarks>
		bool AllowDetachedWindows { get; set; }

		/// <summary>Shows all floating windows.</summary>
		void ShowWindows();

		/// <summary>Hides all floating windows.</summary>
		void HideWindows();
	}
}