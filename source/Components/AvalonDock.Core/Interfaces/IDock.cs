using System.Collections.Generic;

namespace AvalonDock.Core
{
	/// <summary>
	/// Represents a container that holds multiple dockable items.
	/// </summary>
	public interface IDock : IDockable
	{
		IList<IDockable>? VisibleDockables { get; set; }

		IDockable? ActiveDockable { get; set; }

		IDockable? DefaultDockable { get; set; }

		IDockable? FocusedDockable { get; set; }

		bool CanGoBack { get; }

		bool CanGoForward { get; }
	}
}