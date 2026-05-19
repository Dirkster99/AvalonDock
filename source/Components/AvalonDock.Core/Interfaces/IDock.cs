using System.Collections.Generic;

namespace AvalonDock.Core
{
	/// <summary>
	/// Represents a container that holds multiple dockable items.
	/// </summary>
	public interface IDock : IDockable
	{
		/// <summary>Gets or sets the list of currently visible dockable children.</summary>
		IList<IDockable>? VisibleDockables { get; set; }

		/// <summary>Gets or sets the currently active dockable in this container.</summary>
		IDockable? ActiveDockable { get; set; }

		/// <summary>Gets or sets the default dockable to activate when the current one is closed.</summary>
		IDockable? DefaultDockable { get; set; }

		/// <summary>Gets or sets the currently focused dockable.</summary>
		IDockable? FocusedDockable { get; set; }

		/// <summary>Gets a value indicating whether navigation can go back to a previous dockable.</summary>
		bool CanGoBack { get; }

		/// <summary>Gets a value indicating whether navigation can go forward to a next dockable.</summary>
		bool CanGoForward { get; }
	}
}