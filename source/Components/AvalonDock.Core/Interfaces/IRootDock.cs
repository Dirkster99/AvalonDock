using System.Collections.Generic;

namespace AvalonDock.Core
{
	/// <summary>
	/// Represents the root of a dock layout tree.
	/// </summary>
	public interface IRootDock : IDock
	{
		IList<IDockable>? FloatingDockables { get; set; }

		IList<IDockable>? PinnedDockables { get; set; }

		IDockable? DefaultLayout { get; set; }

		void ShowWindows();

		void HideWindows();
	}
}