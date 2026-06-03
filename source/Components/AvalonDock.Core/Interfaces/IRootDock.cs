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

		/// <summary>Shows all floating windows.</summary>
		void ShowWindows();

		/// <summary>Hides all floating windows.</summary>
		void HideWindows();
	}
}