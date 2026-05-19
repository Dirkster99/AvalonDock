namespace AvalonDock.Core
{
	/// <summary>
	/// Event callbacks for factory lifecycle operations.
	/// </summary>
	public abstract partial class FactoryBase
	{
		/// <summary>Called after a dockable has been moved between docks.</summary>
		/// <param name="dockable">The dockable that was moved.</param>
		/// <param name="source">The source dock.</param>
		/// <param name="target">The target dock.</param>
		protected virtual void OnDockableMoved(IDockable dockable, IDock source, IDock target)
		{
		}

		/// <summary>Called after a dockable has been pinned.</summary>
		/// <param name="dockable">The dockable that was pinned.</param>
		protected virtual void OnDockablePinned(IDockable dockable)
		{
		}

		/// <summary>Called after a dockable has been floated.</summary>
		/// <param name="dockable">The dockable that was floated.</param>
		protected virtual void OnDockableFloated(IDockable dockable)
		{
		}

		/// <summary>Called after a dockable has been closed.</summary>
		/// <param name="dockable">The dockable that was closed.</param>
		protected virtual void OnDockableClosed(IDockable dockable)
		{
		}

		/// <summary>Called after a dockable has been added to a dock.</summary>
		/// <param name="dockable">The dockable that was added.</param>
		/// <param name="parent">The parent dock.</param>
		protected virtual void OnDockableAdded(IDockable dockable, IDock parent)
		{
		}

		/// <summary>Called after the active dockable in a dock has changed.</summary>
		/// <param name="dock">The dock whose active dockable changed.</param>
		/// <param name="dockable">The newly active dockable, or null.</param>
		protected virtual void OnActiveDockableChanged(IDock dock, IDockable? dockable)
		{
		}
	}
}